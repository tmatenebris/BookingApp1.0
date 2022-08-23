using System;
using System.Collections.Generic;

namespace BookingApp1._0.Models2
{
    public partial class Bookingview
    {
        public int? BookingId { get; set; }
        public int? UserId { get; set; }
        public int? OwnerId { get; set; }
        public string? Username { get; set; }
        public string? Owner { get; set; }
        public string? Image { get; set; }
        public string? Name { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int? TotalPrice { get; set; }
    }
}
