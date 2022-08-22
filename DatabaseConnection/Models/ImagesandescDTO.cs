using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    public partial class ImagesandescDTO
    {
        public int ImageId { get; set; }
        public int? HallId { get; set; }
        public byte[] Image { get; set; }
        public string? Description { get; set; }
    }
}
