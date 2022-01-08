using System;
using System.Collections.Generic;

namespace Utilitarios
{
    public class FilterStationarySpeedRadar
    {
        public int? EquipmentId { get; set; }
        public int? CustomerInfoId { get; set; }
        public DateTime DateInit { get; set; }
        public DateTime DateFinal { get; set; }
        public int? SubprojectId { get; set; }
        public int? SpeedReportId { get; set; }
        public List<StationarySpeedRadarItemHour> ItemsHour { get; set; }
    }
}
