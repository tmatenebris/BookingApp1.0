using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Database.Models
{
    public partial class User
    {
        public User()
        {
            Bookings = new HashSet<Booking>();
            Halls = new HashSet<Hall>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;

        [XmlIgnore]
        public virtual ICollection<Booking> Bookings { get; set; }

        [XmlIgnore]
        public virtual ICollection<Hall> Halls { get; set; }
    }
}
