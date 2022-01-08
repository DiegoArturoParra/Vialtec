using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("reflectivity", Schema = "reports")]
    public class Reflectivity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("raw_data", TypeName = "varchar")]
        public string RawData { get; set; }

        [Column("activity_id")]
        public int SubprojectId { get; set; }
        public Subproject Subproject { get; set; }

        [Column("server_dt"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}"), Display(Name = "Fecha transmisión")]
        public DateTime ServetDt { get; set; }

        [Column("device_dt"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}"), Display(Name = "Fecha medición")]
        public DateTime DeviceDt { get; set; }

        [Column("measurement"), Display(Name = "Reflectividad")]
        public int Measurement { get; set; }

        [Column("line_number"), Display(Name = "Línea")]
        public int LineNumber { get; set; }

        [Column("line_color_id"), Display(Name = "Color")]
        public int LineColorId { get; set; }
        [Display(Name = "Color")]
        public LineColor LineColor { get; set; }

        [Column("device_serial", TypeName = "varchar"), Display(Name = "Serial Equipo")]
        public string DeviceSerial { get; set; }

        [Column("network_identifier", TypeName = "varchar")]
        public string NetworkIdentifier { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }

        [Column("pr_str", TypeName = "varchar"), Display(Name = "PR")]
        public string PrStr { get; set; }

        [Column("pr_val")]
        public double PrVal { get; set; }

        [Column("latitude"), Display(Name = "Latitud")]
        public double Latitude { get; set; }

        [Column("longitude"), Display(Name = "Longitud")]
        public double Longitude { get; set; }

        [Column("model", TypeName = "varchar"), Display(Name = "Modelo")]
        public string Model { get; set; }

        [Column("geometry_id"), Display(Name = "Geometría")]
        public int GeometryId { get; set; }
        [Display(Name = "Geometría")]
        public Geometry Geometry { get; set; }

        [Column("imported")]
        public bool Imported { get; set; }

        [Column("picture", TypeName = "varchar")]
        public string Picture { get; set; }
    }
}
