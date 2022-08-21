using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Offer
    {
        public int? HallId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int? Price { get; set; }
        public int? Capacity { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }


        public Offer(OfferDTO a)
        {
            this.HallId = a.HallId;
            this.FirstName = a.FirstName;
            this.LastName = a.LastName;
            this.PhoneNumber = a.PhoneNumber;
            this.Email = a.Email;
            this.Name = a.Name;
            this.Location = a.Location;
            this.Price = a.Price;
            this.Capacity = a.Capacity;
            this.Image = Convert.ToBase64String(a.Image);
            this.Description = a.Description;
        }
        public Offer()
        {

        }
    }
}
