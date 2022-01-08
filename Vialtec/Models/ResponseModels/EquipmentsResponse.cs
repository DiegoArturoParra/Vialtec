using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vialtec.Models.ResponseModels
{
    public class EquipmentsResponse
    {
        public int equipment_id { get; set; }
        public string equipment_alias { get; set; }
        public string equipment_description { get; set; }
        public string network_identifier { get; set; }
        public string asset_serial { get; set; }
        public int model_id { get; set; }
        public string model_title { get; set; }
        public string dev_pass { get; set; }
        public string security_data { get; set; }
        public object bluetooth_info { get; set; }
    }
}
