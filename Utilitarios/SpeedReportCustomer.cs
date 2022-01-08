using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("speed_report_by_customer", Schema = "custom_configuration")]
    public class SpeedReportCustomer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }
        
        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }
    }
}
