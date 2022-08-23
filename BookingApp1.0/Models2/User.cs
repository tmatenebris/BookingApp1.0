using System;
using System.Collections.Generic;

namespace BookingApp1._0.Models2
{
    public partial class User
    {
        public User()
        {
            BookingOwners = new HashSet<Booking>();
            BookingUsers = new HashSet<Booking>();
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

        public virtual ICollection<Booking> BookingOwners { get; set; }
        public virtual ICollection<Booking> BookingUsers { get; set; }
        public virtual ICollection<Hall> Halls { get; set; }
    }
}
