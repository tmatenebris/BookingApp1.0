using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using Npgsql;

namespace TCPConnection
{
    public class ClientConnection
    {
        public Socket clientSocket;
        public DbContextOptionsBuilder<BookingAppContext> options;
        public int UserId;
    }


    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public int numero;
    }
    public class TCPServer
    {
        private static List<ClientConnection> clients = new List<ClientConnection>();
        private static AutoResetEvent allDone = new AutoResetEvent(false);
    

        public static void StartListening()
        {

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            List<IPEndPoint> clients = new List<IPEndPoint>();

            Console.WriteLine("SERVER STARTING..");

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                while (true)
                {

                    allDone.Reset();

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);


                    allDone.WaitOne();

                }

            }
            catch (Exception e)
            {

            }



        }

        private static void AcceptCallback(IAsyncResult ar)
        {

            allDone.Set();


            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            ClientConnection new_client = new ClientConnection();
            new_client.clientSocket = handler;
            clients.Add(new_client);

            Console.WriteLine("Client connected");
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
          
            
            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {

                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    ClientConnection current = null;
                    Console.WriteLine(content);
                    foreach (ClientConnection client in clients)
                    {
                        if (handler == client.clientSocket)
                        {
                            current = client;
                            break;
                        }
                    }
                    content = content.Replace("<EOF>", "");
                    if (content.IndexOf("[(ADD_USER)]") != -1)
                    {
                        content = content.Replace("[(ADD_USER)]", "");
                        //Console.WriteLine("Recieved user packagge!");

                        ///string result = databaseConnection.RegisterUser(new_us);
                        string result = "error";
                        //byte[] data = Encoding.ASCII.GetBytes(result);
                        //current.clientSocket.Send(data);
                        Console.WriteLine(content);
                        using (var context = new BookingAppContext())
                        {
                            try
                            {
                                User new_user = XMLSerialize.Deserialize<User>(content);
                                context.Database.ExecuteSqlRaw("create user " + new_user.Username + " with password '" + new_user.Password + "'");
                                context.Database.ExecuteSqlRaw("alter group users add user " + new_user.Username);
                                new_user.Role = "user";

                                context.Users.Add(new_user);

                                context.SaveChanges();

                                result = "succeed";
                                Send(handler, result+"<EOF>");
                            }
                            catch (Exception ex)
                            {
                              
                               Send(handler, result+"<EOF>");
                            }
                        }

                        
                    }
                    else if (content.IndexOf("[(UPDATE)]") > -1)
                    {
                        content = content.Replace("[(UPDATE)]", "");

                        HallDTO new_hallDTO = XMLSerialize.Deserialize<HallDTO>(content);
                        ///string result = current.databaseConnection.InsertHall(new_hall);
                        // byte[] data = Encoding.ASCII.GetBytes(result);
                        // current.clientSocket.Send(data);
                        ///
                        string result = "error";
                        Hall new_hall = new Hall(new_hallDTO);
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                

                                var to_update = context.Halls.Where(s => s.HallId == new_hall.HallId).First();
                                to_update.Name = new_hall.Name;
                                to_update.Location = new_hall.Location;
                                //to_update.Description = new_hall.Description;
                                to_update.Image = new_hall.Image;
                                to_update.Price = new_hall.Price;
                                to_update.Capacity = new_hall.Capacity;
                               
                                context.SaveChanges();

                                result = "succeed";
                                Send(handler, result + "<EOF>");
                            }
                           catch (Exception ex)
                           {
                                Send(handler, result + "<EOF>");
                           }
                        }


                    }
                    else if (content.IndexOf("HallDTO") != -1)
                    {
                       
                        ///string result = current.databaseConnection.InsertHall(new_hall);
                        // byte[] data = Encoding.ASCII.GetBytes(result);
                        // current.clientSocket.Send(data);
                        ///
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                 HallDTO new_hallDTO = XMLSerialize.Deserialize<HallDTO>(content);
                                Hall new_hall = new Hall(new_hallDTO);
                                Imagesandesc new_imgs = new Imagesandesc();
                                new_imgs.Description = new_hallDTO.Description;
                                new_imgs.Image = Convert.ToBase64String(new_hallDTO.Image);
                               
                                context.Halls.Add(new_hall);
                                //context.Add(new_imgs);
                                context.SaveChanges();

                                new_imgs.HallId = new_hall.HallId;
                                context.Add(new_imgs);
                                context.SaveChanges();
                                result = "succeed";
                                Send(handler,result + "<EOF>");
                           }
                            catch (Exception ex)
                           {
                                Send(handler, result + "<EOF>");
                           }
                        }
                    }
                    else if(content.IndexOf("<Booking/>") > -1)
                    {
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                Booking new_booking = XMLSerialize.Deserialize<Booking>(content);
                                context.Bookings.Add(new_booking);
                                context.SaveChanges();

                                result = "succeed";
                                Send(handler, result + "<EOF>");
                            }
                            catch (Exception ex)
                            { 
                                Send(handler, result + "<EOF>");
                            }
                        }
                    }
                    else if (content.IndexOf("GetMyBookings") > -1)
                    {
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                List<BookingView> booking_list = new List<BookingView>();
                                booking_list = context.Bookingviews.Where(s => s.UserId == current.UserId).ToList();
                               
                                result = XMLSerialize.Serialize<List<BookingView>>(booking_list);
                                Console.WriteLine(result);
                                Send(handler, result + "<EOF>");
                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }
                        }
                    }
                    else if (content.IndexOf("GetHallsNotIn: ") != -1)
                    {
                        content = content.Remove(0, 15);
                        int[] ids = content.Split(',').Select(Int32.Parse).ToArray();
                        /// result = current.databaseConnection.GetHallsNotIn(text);
                        ///byte[] data = Encoding.ASCII.GetBytes(result);
                        ///current.clientSocket.Send(data);
                        ///
                        string result = "empty";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                var halls = context.Halls.Where(s => (!ids.Contains(s.HallId))).Take(1).ToList();
                                if (!halls.Any()) throw new ArgumentNullException();
                                List<HallDTO> hallDTOs = new List<HallDTO>();
                                foreach (var hall in halls)
                                {
                                    hallDTOs.Add(new HallDTO(hall));
                                }
                                result = XMLSerialize.Serialize<List<HallDTO>>(hallDTOs);
                                Send(handler, result + "<EOF>");

                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }
                        }
                    }
                    else if (content.IndexOf("[(DELETE_HALL)]:") > -1)
                    {
                        content = content.Replace("[(DELETE_HALL)]:", "");
                        content  = content.Replace("(", "");
                        content = content.Replace(")", "");
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                int hall_id = Convert.ToInt32(content);
                                context.Remove(context.Imagesandescs.Single(s => s.HallId == hall_id));
                                context.SaveChanges();
                                context.Remove(context.Halls.Single(s => s.HallId == hall_id));
                                context.SaveChanges();

                                result = "succeed";
                                Send(handler, result + "<EOF>");
                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }
                        }
                    }
                    else if (content.IndexOf("[(GET_FILTERED_DATA)]") > -1)
                    {
                        content = content.Replace("[(GET_FILTERED_DATA)]", "");
                        Console.WriteLine(content);
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                           // try
                           // {
                                Filters to_apply = XMLSerialize.Deserialize<Filters>(content);
                                
                                DbConnection con = context.Database.GetDbConnection();

                                con.Open();
                                DbCommand cmd = con.CreateCommand();

                                cmd.CommandText = @"select * from getfiltereddata('"+to_apply.from_date+"', '"+to_apply.to_date+"', "+to_apply.from_price+", "+to_apply.to_price+", "+to_apply.from_capacity+ ","+to_apply.to_capacity+", '"+to_apply.location+"')";

                                DbDataReader reader = cmd.ExecuteReader();
                                List<HallDTO> halls = new List<HallDTO>();
                                if(reader.HasRows)
                                {
                                    while(reader.Read())
                                    {
                                        HallDTO hallDTO = new HallDTO();

                                        HallDTO tmp = new HallDTO();
                                        tmp.HallId = reader.GetInt32(0);
                                        tmp.Name = reader.GetString(1);
                                        tmp.Location = reader.GetString(2);
                                        tmp.Price = reader.GetInt32(3);
                                        tmp.Capacity = reader.GetInt32(4);
                                        tmp.ThumbnailImage = Convert.FromBase64String(reader.GetString(5));
                                        halls.Add(tmp);
                                    }
                                }

                                reader.Close();
                                con.Close();
                                result = XMLSerialize.Serialize<List<HallDTO>>(halls);
                                Console.WriteLine(result);
                                Send(handler, result + "<EOF>");

                           // }
                            //catch (Exception ex)
                           // {
                           //     Send(handler, result + "<EOF>");
                            //}
                        }


                    }
                    else if (content == "GetFiltersInitial")
                    {
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                Filters initFilters = new Filters();

                                DbConnection con = context.Database.GetDbConnection();

                                con.Open();
                                DbCommand cmd = con.CreateCommand();

                                cmd.CommandText = "select * from getminmaxprice()";

                                DbDataReader reader = cmd.ExecuteReader();
                                
                                if(reader.HasRows)
                                {
                                    reader.Read();
                                    initFilters.from_price = reader.GetInt32(0);
                                    initFilters.to_price = reader.GetInt32(1);
                                }

                                reader.Dispose();
                                cmd.CommandText = "select * from getminmaxcapacity()";
                                reader = cmd.ExecuteReader();

                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    initFilters.from_capacity = reader.GetInt32(0);
                                    initFilters.to_capacity = reader.GetInt32(1);
                                }

                                reader.Dispose();
                                cmd.CommandText = "select * from getlocations()";
                                reader = cmd.ExecuteReader();


                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        initFilters.locations.Add(reader.GetString(0));
                                    }
                                }

                                //users.ForEach(Console.WriteLine);
                                reader.Close();

                                con.Close();

                                result = XMLSerialize.Serialize<Filters>(initFilters);

                                Console.WriteLine(result);
                                Send(handler, result + "<EOF>");
                            }
                            catch (Exception ex)
                            {
                              Send(handler, result + "<EOF>");
                            }
                        }
                        ///string result = current.databaseConnection.GetFiltersInitial();
                        ///byte[] data = Encoding.ASCII.GetBytes(result);
                        ///current.clientSocket.Send(data);
                    }
                    else if (content == "[(GetUsers)]")
                    {
                        //string result = current.databaseConnection.GetUsers()var query

                        //var query;
                        using (var context = new BookingAppContext())
                        {
                            var query = context.Users.ToList();
                            string result = XMLSerialize.Serialize<List<User>>(query);
                            Console.WriteLine(result);
                            
                            Send(handler, result+"<EOF>");
                        
                        }
                        //string result = XMLSerialize.Deserialize<List<User>>(query);
                        //byte[] data = Encoding.ASCII.GetBytes(result);
                        //current.clientSocket.Send(data);
                    }
                    else if (content == "GetHalls")
                    {
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                var halls = context.Halls.ToList();
                                List<HallDTO> hallDTOs = new List<HallDTO>();
                                foreach (var hall in halls)
                                {
                                    hallDTOs.Add(new HallDTO(hall));
                                }
                                result = XMLSerialize.Serialize<List<HallDTO>>(hallDTOs);
                                Send(handler, result + "<EOF>");

                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }
                        }
                        ///string result = current.databaseConnection.GetHalls();
                        ///byte[] data = Encoding.ASCII.GetBytes(result);
                        //current.clientSocket.Send(data);
                    }
                    else if (content.IndexOf("GetOffer:") != -1)
                    {
                        content = content.Replace("<", "");
                        content = content.Replace(">", "");

                        int hall_id = int.Parse(content.Replace("GetOffer: ", ""));
                        
                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                            try
                            {
                                var offer = context.Offers.Where(s => s.HallId == hall_id).First();
                                OfferDTO offerDTO = new OfferDTO((Offer)offer);

                                result = XMLSerialize.Serialize<OfferDTO>(offerDTO);
                                Send(handler, result + "<EOF>");

                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }
                        }
                    }
                    else if (content.IndexOf("Login:") != -1)
                    {
                        string login = content.Split('<', '>')[1];
                        string password = content.Split('<', '>')[3];
                        Console.WriteLine(login + password);
                        //Console.WriteLine(login);
                        //Console.WriteLine(password);
                        current.options = new DbContextOptionsBuilder<BookingAppContext>();
                        current.options.UseNpgsql("Host=localhost;Database=BookingApp;Username=" + login + ";Password=" + password);
                        //current.databaseConnection.SetupConnection(login, password);
                        ///string uid = databaseConnection.Login(login, password);
                        ///
                        string result = "unknown";
                        using (var context = new BookingAppContext())
                        {
                            //try
                            //{
                            var user = context.Users.Where(s => s.Username == login && s.Password == password).First();

                            result = XMLSerialize.Serialize<User>(user);
                            Console.WriteLine(result);
                                current.UserId = user.UserId;
                            Send(handler, result+"<EOF>");
                           // }
                           // catch(Exception ex)
                            //{
                           //    Send(handler, result + "<EOF>");
                           // }
                        
                        }
                        ///if (uid != "unknown")
                        //{
                        //    UserDTO curr_user = XMLSerialize.Deserialize<UserDTO>(uid);
                        //    current.UserId = curr_user.UserId;
                        //}
                        //byte[] data = Encoding.ASCII.GetBytes(uid);
                        //current.clientSocket.Send(data);

                    }
                    else if (content == "GetIdFromServer")
                    {
                        string result = "error";
                        using (var context = new BookingAppContext())
                        {
                            try
                            {
                                string username = "";

                                DbConnection con = context.Database.GetDbConnection();

                                con.Open();
                                DbCommand cmd = con.CreateCommand();

                                cmd.CommandText = "select current_user";

                                DbDataReader reader = cmd.ExecuteReader();

                                if (reader.HasRows)
                                {
                                    reader.Read();
                                    username = reader.GetString(0);
                                }

                                reader.Close();
                                con.Close();

                                var user = context.Users.Where(s => s.Username == username).First();

                                result = user.UserId.ToString();



                                Send(handler, result + "<EOF>");
                            }
                            catch (Exception ex)
                            {
                                Send(handler, result + "<EOF>");
                            }

                        }

                    }
                    else if(content.IndexOf("GetMyHalls") > -1)
                    {

                        string result = "error";
                        using (var context = new BookingAppContext(current.options.Options))
                        {
                           try
                           {
                                var my_halls = context.Halls.Where(s => s.OwnerId == current.UserId).ToList();

                                List<HallDTO> hallDTOs = new List<HallDTO>();
                                foreach (var hall in my_halls)
                                {
                                    hallDTOs.Add(new HallDTO(hall));
                                }
                                result = XMLSerialize.Serialize<List<HallDTO>>(hallDTOs);
                                Console.WriteLine(result);
                                Send(handler, result + "<EOF>");
                           }
                          catch (Exception ex)
                          {
                                Send(handler, result + "<EOF>");
                          }

                        }

                    }
                    else if (content.ToLower() == "exit") // Client wants to exit gracefully
                    {
                        //Always Shutdown before closing
                        current.clientSocket.Shutdown(SocketShutdown.Both);
                        current.clientSocket.Close();
                        clients.Remove(current);
                        Console.WriteLine("Client disconnected");
                        return;
                    }
                    else
                    {
                        current.clientSocket.Shutdown(SocketShutdown.Both);
                        current.clientSocket.Close();
                        clients.Remove(current);
                        Console.WriteLine("Client disconnected");
                        return;
                    }
                    state.sb.Clear();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {

            byte[] byteData = Encoding.ASCII.GetBytes(data);


            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {

                Socket handler = (Socket)ar.AsyncState;


                int bytesSent = handler.EndSend(ar);
                Console.WriteLine(bytesSent.ToString());
       
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }


}