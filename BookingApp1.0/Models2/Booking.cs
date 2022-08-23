using System;
using System.Collections.Generic;

namespace BookingApp1._0.Models2
{
    public partial class Booking
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public int? HallId { get; set; }
        public int? OwnerId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int? TotalPrice { get; set; }

        public virtual Hall? Hall { get; set; }
        public virtual User? Owner { get; set; }
        public virtual User? User { get; set; }
    }
}
