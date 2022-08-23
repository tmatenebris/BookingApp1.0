using System;
using System.Collections.Generic;

namespace BookingApp1._0.Models2
{
    public partial class Hall
    {
        public Hall()
        {
            Bookings = new HashSet<Booking>();
            Imagesandescs = new HashSet<Imagesandesc>();
        }

        public int HallId { get; set; }
        public int? OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int Price { get; set; }
        public int Capacity { get; set; }
        public string? Image { get; set; }

        public virtual User? Owner { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Imagesandesc> Imagesandescs { get; set; }
    }
}
