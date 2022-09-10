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
     
    }
    public class TCPServer
    {
        private static List<ClientConnection> clients = new List<ClientConnection>();
        private static AutoResetEvent allDone = new AutoResetEvent(false);
    

        private static void AddUser(Socket handler, string content)
        {
            string result = "error";
            
            try
            {
                using (var context = new BookingAppContext())
                {
                   
                    User new_user = XMLSerialize.Deserialize<User>(content);
                    context.Database.ExecuteSqlRaw("create user " + new_user.Username + " with password '" + new_user.Password + "'");
                    context.Database.ExecuteSqlRaw("alter group users add user " + new_user.Username);
                    new_user.Role = "user";
                    context.Users.Add(new_user);
                    context.SaveChanges();
                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.ToString());
               Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void UpdateHall(ClientConnection current, Socket handler, string content)
        {

            string result = "error";
            try
            { 
                using (var context = new BookingAppContext(current.options.Options))
                {
               
                    HallDTO new_hallDTO = XMLSerialize.Deserialize<HallDTO>(content);
                    Hall new_hall = new Hall(new_hallDTO);
                    var hto_update = context.Halls.Where(s => s.HallId == new_hall.HallId).First();

                    hto_update.Name = new_hall.Name;
                    hto_update.Location = new_hall.Location;
                    hto_update.Price = new_hall.Price;
                    hto_update.Capacity = new_hall.Capacity;
                    hto_update.Image = new_hall.Image;
                    context.SaveChanges();

                    var ito_update = context.Imagesandescs.Where(s => s.HallId == new_hall.HallId).First();
                    ito_update.Description = new_hallDTO.Description;
                    ito_update.Image = Convert.ToBase64String(new_hallDTO.Image);

                    context.SaveChanges();
                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }

            Console.WriteLine(content);
        }

        private static void AddHall(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
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
                    Send(handler, result + "<EOF>");
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void AddBooking(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                { 
                    Booking new_booking = XMLSerialize.Deserialize<Booking>(content);

                    DbConnection con = context.Database.GetDbConnection();

                    con.Open();
                    DbCommand cmd = con.CreateCommand();
                    Console.WriteLine(new_booking.FromDateString);
                    Console.WriteLine(new_booking.ToDateString);
                    cmd.CommandText = "select * from isavailable(" + new_booking.HallId + ",'" + new_booking.FromDateString + "', '" + new_booking.ToDateString + "')";

                    DbDataReader reader = cmd.ExecuteReader();
                    int status = 0;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        status = reader.GetInt32(0);

                    }
                    if (status != 0) throw new Exception();

                    reader.Close();
                    con.Close();


                    context.Bookings.Add(new_booking);
                    context.SaveChanges();

                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }


        private static void GetUserBookings(ClientConnection current, Socket handler, string content)
        {

            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                {
                    List<BookingView> booking_list = new List<BookingView>();
                    booking_list = context.Bookingviews.Where(s => s.UserId == current.UserId).ToList();

                    result = XMLSerialize.Serialize<List<BookingView>>(booking_list);
                    Console.WriteLine(result);
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void GetAllBookings(Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext())
                {
                    List<BookingView> booking_list = new List<BookingView>();
                    booking_list = context.Bookingviews.ToList();

                    result = XMLSerialize.Serialize<List<BookingView>>(booking_list);
                    Console.WriteLine(result);
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            
        }

        private static void DeleteHall(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
              try
              {
                using (var context = new BookingAppContext(current.options.Options))
                {
                    int hall_id = Convert.ToInt32(content);
                    context.Remove(context.Imagesandescs.Single(s => s.HallId == hall_id));
                    context.SaveChanges();
                    context.Remove(context.Halls.Single(s => s.HallId == hall_id));
                    context.SaveChanges();

                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }
              }
              catch (Exception ex)
              {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
              }

            Console.WriteLine(content);
        }


        private static void DeleteBooking(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                {
                    int booking_id = Convert.ToInt32(content);
                    context.Remove(context.Bookings.Single(s => s.BookingId == booking_id));
                    context.SaveChanges();

                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void DeleteUser(Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext())
                {
                    int uid = Convert.ToInt32(content);
                    User user = context.Users.Where(s => s.UserId == uid).First();
                    if (user.Role == "admin") throw new Exception();

                    context.Database.ExecuteSqlRaw("call drop_user(" + uid + ")");
                    context.Database.ExecuteSqlRaw("drop user " + user.Username);

                    context.SaveChanges();

                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void GetFilteredData(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                {

                    Filters to_apply = XMLSerialize.Deserialize<Filters>(content);

                    DbConnection con = context.Database.GetDbConnection();

                    con.Open();
                    DbCommand cmd = con.CreateCommand();

                    if (to_apply.userid != -1)
                    {
                        cmd.CommandText = @"select * from getuserfiltereddata('" + to_apply.from_date + "', '" + to_apply.to_date + "', " + to_apply.from_price + ", " + to_apply.to_price + ", " + to_apply.from_capacity + "," + to_apply.to_capacity + ", '" + to_apply.location + "', " + to_apply.userid + ")";
                    }
                    else cmd.CommandText = @"select * from getfiltereddata('" + to_apply.from_date + "', '" + to_apply.to_date + "', " + to_apply.from_price + ", " + to_apply.to_price + ", " + to_apply.from_capacity + "," + to_apply.to_capacity + ", '" + to_apply.location + "')";

                    DbDataReader reader = cmd.ExecuteReader();
                    List<HallDTO> halls = new List<HallDTO>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void GetFiltersInitial(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                {

                    Filters initFilters = new Filters();

                    DbConnection con = context.Database.GetDbConnection();

                    con.Open();
                    DbCommand cmd = con.CreateCommand();

                    cmd.CommandText = "select * from getminmaxprice()";

                    DbDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
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

                    reader.Close();

                    con.Close();

                    result = XMLSerialize.Serialize<Filters>(initFilters);

                    Console.WriteLine(result);
                    Send(handler, result + "<EOF>");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void GetUserFiltersInitial(ClientConnection current, Socket handler, string content)
        {
            string uid = content;
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
                {
                    Filters initFilters = new Filters();

                    DbConnection con = context.Database.GetDbConnection();

                    con.Open();
                    DbCommand cmd = con.CreateCommand();

                    cmd.CommandText = "select * from getuserminmaxprice('" + uid + "')";

                    DbDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        initFilters.from_price = reader.GetInt32(0);
                        initFilters.to_price = reader.GetInt32(1);
                    }

                    reader.Dispose();
                    cmd.CommandText = "select * from getuserminmaxcapacity('" + uid + "')";
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        initFilters.from_capacity = reader.GetInt32(0);
                        initFilters.to_capacity = reader.GetInt32(1);
                    }

                    reader.Dispose();
                    cmd.CommandText = "select * from getuserlocations('" + uid + "')";
                    reader = cmd.ExecuteReader();


                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            initFilters.locations.Add(reader.GetString(0));
                        }
                    }
                    reader.Close();

                    con.Close();

                    result = XMLSerialize.Serialize<Filters>(initFilters);

                    Console.WriteLine(result);
                    Send(handler, result + "<EOF>");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void GetUsers(Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext())
                {
                    var query = context.Users.ToList();
                    result = XMLSerialize.Serialize<List<User>>(query);
                    Console.WriteLine(result);
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }


        private static void GetHalls(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }


        private static void GetOffer(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                int hall_id = int.Parse(content);
                using (var context = new BookingAppContext(current.options.Options))
                {
                    var offer = context.Offers.Where(s => s.HallId == hall_id).First();
                    OfferDTO offerDTO = new OfferDTO((Offer)offer);

                    result = XMLSerialize.Serialize<OfferDTO>(offerDTO);
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }

        private static void Login(ClientConnection current, Socket handler, string content)
        {
            string login = content.Split('<', '>')[1];
            string password = content.Split('<', '>')[3];
            Console.WriteLine(login + password);

            current.options = new DbContextOptionsBuilder<BookingAppContext>();
            current.options.UseNpgsql("Host=localhost;Database=BookingApp;Username=" + login + ";Password=" + password);

            string result = "error";
            try
            {
               
                using (var context = new BookingAppContext())
                {
                    context.Database.ExecuteSqlRaw("call clear_bookings()");
                    context.SaveChanges();
                    var user = context.Users.Where(s => s.Username == login && s.Password == password).First();

                    result = XMLSerialize.Serialize<User>(user);
                    Console.WriteLine(result);
                    current.UserId = user.UserId;
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Send(handler, result + "<EOF>");
            }

            Console.WriteLine(content);
        }

        private static void GetUserHalls(ClientConnection current, Socket handler, string content)
        {
            string result = "error";
            try
            {
                using (var context = new BookingAppContext(current.options.Options))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Send(handler, result + "<EOF>");
            }

            Console.WriteLine(content);
        }

        private static void UpdateUser(Socket handler, string content)
        {
            string result = "error";

            try
            {
                using (var context = new BookingAppContext())
                {

                    User new_user = XMLSerialize.Deserialize<User>(content);

                    var user = context.Users.Where(s => s.UserId == new_user.UserId).First();

                    user.FirstName = new_user.FirstName;
                    user.LastName = new_user.LastName;
                    user.Email = new_user.Email;
                    user.PhoneNumber = new_user.PhoneNumber;
          
                    context.SaveChanges();
                    result = "succeed";
                    Send(handler, result + "<EOF>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Send(handler, result + "<EOF>");
            }
            Console.WriteLine(content);
        }
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
                Console.WriteLine("OK");
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
            StateObject state = null;
            Socket handler = null;
            int bytesRead = 0;
            try
            {
                content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                state = (StateObject)ar.AsyncState;
                handler = state.workSocket;


                // Read data from the client socket.
                bytesRead = handler.EndReceive(ar);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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
                            AddUser(handler, content);
                        }
                        else if (content.IndexOf("[(UPDATE_HALL)]") > -1)
                        {
                            content = content.Replace("[(UPDATE_HALL)]", "");
                            UpdateHall(current, handler, content);

                        }
                        else if (content.IndexOf("[(ADD_HALL)]") != -1)
                        {
                            content = content.Replace("[(ADD_HALL)]", "");
                            AddHall(current, handler, content);

                        }
                        else if (content.IndexOf("[(ADD_BOOKING)]") > -1)
                        {
                            content = content.Replace("[(ADD_BOOKING)]", "");
                            AddBooking(current, handler, content);
                        }
                        else if (content.IndexOf("[(GET_MY_BOOKINGS)]") > -1)
                        {
                            content = content.Replace("[(GET_MY_BOOKINGS)]", "");
                            GetUserBookings(current, handler, content);
                        }
                        else if (content.IndexOf("[(GET_ALL_BOOKINGS)]") > -1)
                        {
                            content = content.Replace("[(GET_ALL_BOOKINGS)]", "");
                            GetAllBookings(handler, content);
                        }
                        else if (content.IndexOf("[(DELETE_HALL)]:") > -1)
                        {
                            content = content.Replace("[(DELETE_HALL)]:", "");
                            content = content.Replace("(", "");
                            content = content.Replace(")", "");
                            DeleteHall(current, handler, content);
                        }
                        else if (content.IndexOf("[(DELETE_BOOKING)]:") > -1)
                        {
                            content = content.Replace("[(DELETE_BOOKING)]:", "");
                            content = content.Replace("(", "");
                            content = content.Replace(")", "");
                            DeleteBooking(current, handler, content);
                        }
                        else if (content.IndexOf("[(DELETE_USER)]:") > -1)
                        {
                            content = content.Replace("[(DELETE_USER)]:", "");
                            content = content.Replace("(", "");
                            content = content.Replace(")", "");
                            DeleteUser(handler, content);
                        }
                        else if (content.IndexOf("[(UPDATE_USER)]") > -1)
                        {
                            content = content.Replace("[(UPDATE_USER)]", "");
                            UpdateUser(handler, content);
                        }
                        else if (content.IndexOf("[(GET_FILTERED_DATA)]") > -1)
                        {
                            content = content.Replace("[(GET_FILTERED_DATA)]", "");
                            GetFilteredData(current, handler, content);
                        }
                        else if (content == "[(GET_FILTERS_INITIAL)]")
                        {
                            GetFiltersInitial(current, handler, content);
                        }
                        else if (content.IndexOf("[(GET_USER_FILTERS_INITIAL)]:") > -1)
                        {
                            content = content.Replace("[(GET_USER_FILTERS_INITIAL)]:", "");
                            content = content.Replace("(", "");
                            content = content.Replace(")", "");
                            GetUserFiltersInitial(current, handler, content);
                        }
                        else if (content == "[(GET_USERS)]")
                        {
                            GetUsers(handler, content);
                        }
                        else if (content == "[(GET_HALLS)]")
                        {
                            GetHalls(current, handler, content);
                        }
                        else if (content.IndexOf("[(GET_OFFER)]:") != -1)
                        {
                            content = content.Replace("[(GET_OFFER)]:", "");
                            content = content.Replace("(", "");
                            content = content.Replace(")", "");
                            GetOffer(current, handler, content);
                        }
                        else if (content.IndexOf("[(LOGIN)]:") != -1)
                        {
                            Login(current, handler, content);
                        }
                        else if (content.IndexOf("[(GET_MY_HALLS)]") > -1)
                        {
                            GetUserHalls(current, handler, content);
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