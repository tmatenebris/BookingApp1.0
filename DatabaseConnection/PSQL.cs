using Npgsql;
using System.Data;
using Database.Models;
using System.Diagnostics;
using System.Text;

namespace Database
{
    public class PSQL
    {
        private string connstring = String.Format("Server=localhost;Port=5432;User Id=postgres;Password=postgres;Database=BookingApp;");
        private NpgsqlConnection conn;
        private string sql;
        private NpgsqlCommand cmd;
        private DataTable dt;

        public PSQL()
        {

        }

        public void SetupConnection()
        {
            conn = new NpgsqlConnection(connstring);
        }

        public void SetupConnection(string login, string password)
        {
            conn = new NpgsqlConnection(String.Format("Server=localhost;Port=5432;User Id={0};Password={1};Database=BookingApp;", login, password));
        }

        public string RegisterUser(UserDTO new_user)
        {
            //this.CreateUser(new_user.Username, new_user.Password);
            try
            {
                conn.Open();
                sql = @"create user " + new_user.Username + " with password '" + new_user.Password + "'";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();


                cmd.CommandText = "alter group users add user " + new_user.Username;
                cmd.ExecuteNonQuery();

                sql = @"insert into users (username, password, first_name, last_name, phone_number, email, role) 
            VALUES('" + new_user.Username + "', '" + new_user.Password + "', '" + new_user.FirstName + "', '" + new_user.LastName + "', '" + new_user.PhoneNumber + "', '" + new_user.Email + "', 'user')";
                Console.WriteLine(sql);

                cmd.CommandText = sql;

                cmd.ExecuteNonQuery();
                conn.Close();

                return "succeed";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            conn.Close();
            return "error";
        }



        public void Select()
        {
            try
            {
                conn.Open();

                sql = @"select * from users";
                cmd = new NpgsqlCommand(sql, conn);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            conn.Close();
        }
        /*
        public string InsertHall(HallDTO new_hall)
        {
            try
            {
                conn.Open();
                string hex_img = Convert.ToBase64String(new_hall.Image);
                //string insert_query = "insert into halls(name, location, price, capacity, image, description) Values(@name, @location, @price, @capacity, @image, @description)";
                sql = @"insert into halls (owner_id, name, location, price, capacity, image, description) Values(" + new_hall.OwnerId + ", '" + new_hall.Name + "', '" + new_hall.Location + "' ,"
                    + new_hall.Price + ", " + new_hall.Capacity + ", '" + hex_img + "', '" + new_hall.Description + "')";

                //sql = @"insert into halls (name, location, price, capacity, description) Values('" + new_hall.Name + "', '" + new_hall.Location + "' ,"
                //    + new_hall.Price + ", " + new_hall.Capacity + ",'" + new_hall.Description + "')";
                Console.WriteLine(sql);
                cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                return "success";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            conn.Close();

            return "error";
        }
        */
        public string GetOffer(string hall_id)
        {
            try
            {
                conn.Open();

                sql = @"select hall_id, first_name, last_name, phone_number, email, name, location, price, capacity, image, description from offer where hall_id=" + hall_id;
                cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                OfferDTO offer = new OfferDTO();

                if (reader.HasRows)
                {
                    reader.Read();
                    offer.HallId = reader.GetInt32(0);
                    offer.FirstName = reader.GetString(1);
                    offer.LastName = reader.GetString(2);
                    offer.PhoneNumber = reader.GetString(3);
                    offer.Email = reader.GetString(4);
                    offer.Name = reader.GetString(5);
                    offer.Location = reader.GetString(6);
                    offer.Price = reader.GetInt32(7);
                    offer.Capacity = reader.GetInt32(8);
                    offer.Image = Convert.FromBase64String(reader.GetString(9));
                    offer.Description = reader.GetString(10);

                }

                //users.ForEach(Console.WriteLine);
                
                string value = "";
                if (reader.HasRows) value = XMLSerialize.Serialize<OfferDTO>(offer);
                else value = "error";
                Console.WriteLine(value);
                reader.Close();
                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }
        public string GetUsers()
        {
            try
            {
                conn.Open();

                sql = @"select first_name, last_name, phone_number, email from users";
                cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                List<UserDTO> users = new List<UserDTO>();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        UserDTO tmp = new UserDTO();
                        tmp.FirstName = reader.GetString(0);
                        tmp.LastName = reader.GetString(1);
                        tmp.PhoneNumber = reader.GetString(2);
                        tmp.Email = reader.GetString(3);
                        //Console.WriteLine(tmp.FirstName);
                        //Console.WriteLine(tmp.LastName);
                        //Console.WriteLine(tmp.PhoneNumber);
                        //Console.WriteLine(tmp.Email);

                        users.Add(tmp);
                    }
                }
                users.ForEach(Console.WriteLine);

                string value = "";
                Console.WriteLine(value);
                if (reader.HasRows) value = XMLSerialize.Serialize<List<UserDTO>>(users);
                else value = "error";

                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }

        /*
        public string GetFiltredData(Filters to_apply)
        {
            try
            {
                conn.Open();

                sql = @"select * from getfiltereddata ('" + to_apply.from_date + "', '" + to_apply.to_date + "', "
                    + to_apply.from_price.ToString() + ", " + to_apply.to_price.ToString() + ", " + to_apply.from_capacity.ToString()
                    + ", " + to_apply.to_capacity.ToString() + ", '" + to_apply.location + "', '" + to_apply.idsstring + "')";
                cmd = new NpgsqlCommand(sql, conn);
                Console.WriteLine(sql);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                List<HallDTO> halls = new List<HallDTO>();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        HallDTO tmp = new HallDTO();
                        tmp.HallId = reader.GetInt32(0);
                        tmp.Name = reader.GetString(1);
                        tmp.Location = reader.GetString(2);
                        tmp.Price = reader.GetInt32(3);
                        tmp.Capacity = reader.GetInt32(4);
                        tmp.Image = Convert.FromBase64String(reader.GetString(5));
                        //Console.WriteLine(tmp.FirstName);
                        //Console.WriteLine(tmp.LastName);
                        //Console.WriteLine(tmp.PhoneNumber);
                        //Console.WriteLine(tmp.Email);

                        halls.Add(tmp);
                    }
                }
                //users.ForEach(Console.WriteLine);

                string value = "";
                if (reader.HasRows) value = XMLSerialize.Serialize<List<HallDTO>>(halls);
                else value = "empty";
                Console.WriteLine(value);
                reader.Close();

                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }
        */
        public string GetFiltersInitial()
        {
            try
            {
                conn.Open();


                Filters initFilters = new Filters();
                sql = @"select * from getminmaxprice()";
                cmd = new NpgsqlCommand(sql, conn);
                Console.WriteLine(sql);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    initFilters.from_price = reader.GetInt32(0);
                    initFilters.to_price = reader.GetInt32(1);

                }

                reader.Dispose();
                sql = @"select * from getminmaxcapacity()";
                cmd.CommandText = sql;
                Console.WriteLine(sql);
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    initFilters.from_capacity = reader.GetInt32(0);
                    initFilters.to_capacity = reader.GetInt32(1);

                }

                reader.Dispose();
                sql = @"select getlocations()";
                cmd.CommandText = sql;
                Console.WriteLine(sql);

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
                string value = XMLSerialize.Serialize<Filters>(initFilters);
                Console.WriteLine(value);

                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }

        /*
        public string GetHallsNotIn(string list)
        {
            try
            {
                conn.Open();

                sql = @"select hall_id, name, location, price, capacity, image from halls where hall_id not in " + list + " limit 14";
                cmd = new NpgsqlCommand(sql, conn);
                Console.WriteLine(sql);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                List<HallDTO> halls = new List<HallDTO>();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        HallDTO tmp = new HallDTO();
                        tmp.HallId = reader.GetInt32(0);
                        tmp.Name = reader.GetString(1);
                        tmp.Location = reader.GetString(2);
                        tmp.Price = reader.GetInt32(3);
                        tmp.Capacity = reader.GetInt32(4);
                        tmp.Image = Convert.FromBase64String(reader.GetString(5));
                        //Console.WriteLine(tmp.FirstName);
                        //Console.WriteLine(tmp.LastName);
                        //Console.WriteLine(tmp.PhoneNumber);
                        //Console.WriteLine(tmp.Email);

                        halls.Add(tmp);
                    }
                }
                //users.ForEach(Console.WriteLine);
                string value;
                if (reader.HasRows) value = XMLSerialize.Serialize<List<HallDTO>>(halls);
                else value = "empty";
                reader.Close();
                Console.WriteLine(value);

                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }
        */
        /*
        public string GetHalls()
        {
            try
            {
                conn.Open();

                sql = @"select hall_id, name, location, price, capacity, image from halls limit 14";
                cmd = new NpgsqlCommand(sql, conn);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                List<HallDTO> halls = new List<HallDTO>();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        HallDTO tmp = new HallDTO();
                        tmp.HallId = reader.GetInt32(0);
                        tmp.Name = reader.GetString(1);
                        tmp.Location = reader.GetString(2);
                        tmp.Price = reader.GetInt32(3);
                        tmp.Capacity = reader.GetInt32(4);
                        tmp.Image = Convert.FromBase64String(reader.GetString(5));
                        //Console.WriteLine(tmp.FirstName);
                        //Console.WriteLine(tmp.LastName);
                        //Console.WriteLine(tmp.PhoneNumber);
                        //Console.WriteLine(tmp.Email);

                        halls.Add(tmp);
                    }
                }
                //users.ForEach(Console.WriteLine);
                reader.Close();
                string value = XMLSerialize.Serialize<List<HallDTO>>(halls);
                // Console.WriteLine(value);

                conn.Close();
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            conn.Close();
            return "error";
        }
       */
        public string Login(string username, string password)
        {
            try
            {
                conn.Open();

                sql = @"select user_id, first_name, last_name, phone_number, email, role from users where username='" + username + "' and password='" + password + "'";
                cmd = new NpgsqlCommand(sql, conn);
                //string value = cmd.ExecuteScalar().ToString();
                NpgsqlDataReader reader = cmd.ExecuteReader();
                UserDTO res_user = new UserDTO();

                if (reader.HasRows)
                {
                    reader.Read();
                    res_user.UserId = reader.GetInt32(0);
                    res_user.FirstName = reader.GetString(1);
                    res_user.LastName = reader.GetString(2);
                    res_user.PhoneNumber = reader.GetString(3);
                    res_user.Email = reader.GetString(4);
                    res_user.Role = reader.GetString(5);
                    Console.WriteLine(res_user.UserId);
                    Console.WriteLine(res_user.FirstName);
                    Console.WriteLine(res_user.LastName);
                    Console.WriteLine(res_user.PhoneNumber);
                    Console.WriteLine(res_user.Email);
                    Console.WriteLine(res_user.Role);
                }
                
                
                string value = "unknown";
                if(reader.HasRows) value = XMLSerialize.Serialize<UserDTO>(res_user);
                reader.Close();
                //Console.WriteLine(value);
                conn.Close();
                return value;
              }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            conn.Close();
            return "unknown";
        }

    }

    
}