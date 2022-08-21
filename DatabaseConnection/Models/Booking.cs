using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Booking
    {
        public int BookingId { get; set; }
        public int? HallId { get; set; }
        public int? OwnerId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public virtual Hall? Hall { get; set; }
        public virtual User? Owner { get; set; }
    }
}
