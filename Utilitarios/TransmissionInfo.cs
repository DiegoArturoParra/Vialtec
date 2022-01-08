using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("transmission_info", Schema = "transmission")]
    public class TransmissionInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("raw_data")]
        public string RawData { get; set; }

        [Column("asset_data", TypeName = "json")]
        public string AssetData { get; set; }

        [Column("server_dt"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}"), Display(Name = "Fecha servidor")]
        public DateTime ServerDt { get; set; }

        [Column("device_dt"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}"), Display(Name = "Fecha dispositivo")]
        public DateTime DeviceDt { get; set; }

        [Column("equipment_id")]
        public int? EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        [Column("event_id")]
        public int? EventId { get; set; }
        [Display(Name = "Evento")]
        public Event Event { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("gps_valid"), Display(Name = "GPS")]
        public bool? GpsValid { get; set; }

        [NotMapped]
        [Display(Name = "Evento")]
        public string EventAlias { get; set; }
    }
}
