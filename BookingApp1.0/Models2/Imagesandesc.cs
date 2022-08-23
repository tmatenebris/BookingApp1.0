using System;
using System.Collections.Generic;

namespace BookingApp1._0.Models2
{
    public partial class Imagesandesc
    {
        public int ImageId { get; set; }
        public int? HallId { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }

        public virtual Hall? Hall { get; set; }
    }
}
