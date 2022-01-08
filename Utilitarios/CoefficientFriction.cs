using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("coefficient_friction", Schema = "reports")]
    public class CoefficientFriction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("date"), Display(Name = "Fecha")]
        public DateTime? Date { get; set; }

        [Column("latitude"), Display(Name = "Latitud")]
        public double Latitude { get; set; }

        [Column("longitude"), Display(Name = "Longitud")]
        public double Longitude { get; set; }

        [Column("mu"), Display(Name = "Mu")]
        public double Mu { get; set; }

        [Column("temperature_via"), Display(Name = "Temperatura Vía")]
        public double TemperatureVia { get; set; }

        [Column("temperature_environment"), Display(Name = "Temperatura Ambiente")]
        public double TemperatureEnvironment { get; set; }

        [Column("speed"), Display(Name = "Velocidad")]
        public int Speed { get; set; }

        [Column("pr_str"), Display(Name = "PR")]
        public string PrStr { get; set; }

        [Column("odometer"), Display(Name = "Odómetro")]
        public double Odometer { get; set; }

        [Column("mu_report_id"), Display(Name = "Reporte")]
        public int MuReportId { get; set; }
        [Display(Name = "Reporte")]
        public MuReport MuReport { get; set; }
    }
}
