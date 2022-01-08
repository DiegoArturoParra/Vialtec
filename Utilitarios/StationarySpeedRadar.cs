using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("stationary_speed_radar", Schema = "reports")]
    public class StationarySpeedRadar
    {
        [Column("device_dt"), Display(Name = "Fecha dispositivo")]
        public DateTime DeviceDt { get; set; }

        [Column("server_dt"), Display(Name = "Fecha servidor")]
        public DateTime ServerDt { get; set; }

        [Column("speed"), Display(Name = "Velocidad")]
        public int Speed { get; set; }

        [Column("asset_equipment_id"), Display(Name = "Dispositivo")]
        public int EquipmentId { get; set; }
        [Display(Name = "Dispositivo")]
        public Equipment Equipment { get; set; }

        [Column("vehicle_type_id"), Display(Name = "Categoría")]
        public int VehicleTypeId { get; set; }
        [Display(Name = "Categoría")]
        public virtual VehicleType VehicleType { get; set; }

        [Column("subproject_id"), Display(Name = "Actividad")]
        public int? SubprojectId { get; set; }
        [Display(Name = "Actividad")]
        public Subproject Subproject { get; set; }
    }
}
