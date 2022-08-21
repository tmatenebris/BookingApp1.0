using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    public class Filters
    {
        public string from_date { get;set; }
        public string to_date { get;set; }
        public int from_price { get;set; }
        public int to_price { get;set; }
        public int from_capacity { get;set; }
        public int to_capacity { get; set; }

        public string location { get; set; }

        public string idsstring { get; set; }
        public List<string> locations = new List<string>();
    }

    public class MinMax
    {
        public int minprice { get; set; }
        public int max { get; set; }
    }
}
