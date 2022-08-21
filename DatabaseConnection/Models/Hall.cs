using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Hall
    {
        public Hall()
        {
            Bookings = new HashSet<Booking>();
        }

        public Hall(HallDTO a)
        {
            this.HallId = a.HallId;
            this.OwnerId = a.OwnerId;
            this.Name = a.Name;
            this.Location = a.Location;
            this.Price = a.Price;
            this.Capacity = a.Capacity;
            this.Description = a.Description;
            this.Image = Convert.ToBase64String(a.Image);
        }

        public int HallId { get; set; }
        public int? OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int Price { get; set; }
        public int Capacity { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }

        public virtual User? Owner { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
