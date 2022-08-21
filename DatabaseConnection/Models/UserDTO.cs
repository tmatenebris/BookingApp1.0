﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
  
    public partial class UserDTO
    {
      
        public int UserId { get; set; }

       
        public string Username { get; set; } = null!;
        
        public string Password { get; set; } = null!;
       
        public string FirstName { get; set; } = null!;
        
        public string LastName { get; set; } = null!;
       
        public string PhoneNumber { get; set; } = null!;
     
        public string Email { get; set; } = null!;
      
        public string Role { get; set; } = null!;
    }
}
