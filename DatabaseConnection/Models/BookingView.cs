using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Database.Models
{
    public partial class BookingView
    {
        public int? BookingId { get; set; }
        public int? UserId { get; set; }

        public string? Username { get; set; }
        public int? OwnerId { get; set; }
        public string? Owner { get; set; }

        public int? TotalPrice { get; set; }

        [XmlIgnore]
        public string? Image { get; set; }
        public string? Name { get; set; }

        [XmlIgnore]
        public DateOnly? FromDate { get; set; }

        [XmlIgnore]
        public DateOnly? ToDate { get; set; }


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

        [NotMapped]
        public byte[] ByteImage
        {
            get { return Convert.FromBase64String(Image); }
            set { Image = Convert.ToBase64String(value); }

        }
    }
}
