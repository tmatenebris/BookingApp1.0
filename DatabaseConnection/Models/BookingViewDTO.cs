using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    public partial class BookingviewDTO
    {
        public int? UserId { get; set; }
        public int? OwnerId { get; set; }
        public string? Owner { get; set; }
        public byte[] Image { get; set; }
        public string? Name { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }
}
