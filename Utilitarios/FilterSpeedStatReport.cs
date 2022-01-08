using System;

namespace Utilitarios
{
    public class FilterSpeedStatReport
    {
        public int? EquipmentId { get; set; }
        public int? CustomerInfoId { get; set; }
        public DateTime DateInit { get; set; }
        public DateTime DateFinal { get; set; }
    }
}
