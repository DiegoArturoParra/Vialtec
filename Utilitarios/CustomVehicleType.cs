using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("custom_vehicle_type", Schema = "custom_configuration")]
    public class CustomVehicleType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("vehicle_type_id"), Display(Name = "Tipo vehículo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int VehicleTypeId { get; set; }
        [Display(Name = "Tipo vehículo")]
        public VehicleType VehicleType { get; set; }

        [Column("speed_report_by_customer_id"), Display(Name = "Reporte")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int SpeedReportCustomerId { get; set; }
        [Display(Name = "Reporte")]
        public SpeedReportCustomer SpeedReportCustomer { get; set; }

        [Column("custom_title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string CustomTitle { get; set; }

        [Column("picture"), Display(Name = "Imagen")]
        public string Picture { get; set; }
    }
}
