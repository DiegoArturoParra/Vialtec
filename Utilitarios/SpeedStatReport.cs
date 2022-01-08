using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("speed_stat_report", Schema = "reports")]
    public class SpeedStatReport
    {
        [Column("server_dt"), Display(Name = "Fecha Servidor")]
        public DateTime ServerDt { get; set; }

        [Column("asset_equipment_id")]
        public int EquipmentId { get; set; }
        [Display(Name = "Dispositivo")]
        public Equipment Equipment { get; set; }

        [Column("vehicle_type_id")]
        public int VehicleTypeId { get; set; }
        [Display(Name = "Tipo Vehículo")]
        public VehicleType VehicleType { get; set; }

        [Column("average_speed"), Display(Name = "Promedio Velocidad")]
        public int AverageSpeed { get; set; }

        [Column("last_speed"), Display(Name = "Última Velocidad")]
        public int LastSpeed { get; set; }

        [Column("peak_speed"), Display(Name = "Pico Velocidad")]
        public int PeakSpeed { get; set; }

        [Column("duration"), Display(Name = "Duración")]
        public double Duration { get; set; }

        [Column("device_dt"), Display(Name = "Fecha Dispositivo")]
        public DateTime DeviceDt { get; set; }
    }
}
