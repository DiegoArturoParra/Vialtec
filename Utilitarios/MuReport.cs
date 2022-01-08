using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("mu_report", Schema = "reports")]
    public class MuReport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        public string Title { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }
    }
}
