using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Database.Models
{
    public partial class Booking
    {
        public int BookingId { get; set; }
        public int? UserId { get; set; }
        public int? HallId { get; set; }
        public int? OwnerId { get; set; }

        public int? TotalPrice { get; set; }

        [XmlIgnore]
        public DateOnly FromDate { get; set; }

        [XmlIgnore]
        public DateOnly ToDate { get; set; }

        [NotMapped]
        public string FromDateString
        {
            get { return FromDate.ToString(); }
            set { FromDate = DateOnly.Parse(value); }
        }
        [NotMapped]
        public string ToDateString
        {
            get { return ToDate.ToString(); }
            set { ToDate = DateOnly.Parse(value); }
        }

        [XmlIgnore]
        public virtual Hall? Hall { get; set; }
        [XmlIgnore]
        public virtual User? Owner { get; set; }
        [XmlIgnore]
        public virtual User? User { get; set; }
    }
}
