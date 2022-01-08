using System;

namespace Api.Models
{
    public class RawData
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime datetime { get; set; }
        public int measurement { get; set; }
        public int pr_km { get; set; }
        public int pr_mt { get; set; }
        public int geometry { get; set; }
        public int color { get; set; }
        public int line { get; set; }
        public string serial { get; set; }
        public string model { get; set; }
        public string network_identifier { get; set; }
        public string picture { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public int activity { get; set; }
    }
}
