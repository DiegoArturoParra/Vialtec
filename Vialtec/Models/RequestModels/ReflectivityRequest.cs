using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vialtec.Models.RequestModels
{
    public class ReflectivityRequest
    {
        public string user { get; set; }

        public string pass { get; set; }

        public string network_identifier { get; set; }

        public string datetime { get; set; }

        // RAW DATA

        public double? latitude { get; set; }

        public double? longitude { get; set; }

        public double? pr_km { get; set; }

        public double? pr_mt { get; set; }

        public int? color { get; set; }

        public int? line { get; set; }

        public int? measurement { get; set; }

        public string serial { get; set; }

        public int? activity { get; set; }

        public string model { get; set; }

        public int? geometry { get; set; }

        public string picture { get; set; }
    }
}
