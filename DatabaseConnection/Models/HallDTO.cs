using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Database.Models
{
    public partial class HallDTO
    {

        public int HallId { get; set; }
        public int? OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int Price { get; set; }
        public int Capacity { get; set; }
        public byte[] Image { get; set; }
        public string? Description { get; set; }

  
        public HallDTO(Hall a)
        {
            this.HallId = a.HallId;
            this.OwnerId = a.OwnerId;
            this.Name = a.Name;
            this.Location = a.Location;
            this.Price = a.Price;
            this.Capacity = a.Capacity;
            this.Description = a.Description;
            this.Image = Convert.FromBase64String(a.Image);
        }
        public HallDTO()
        {

        }
    }
}