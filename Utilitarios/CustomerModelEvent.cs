using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("customer_model_event", Schema = "customer")]
    public class CustomerModelEvent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("model_event_id"), Display(Name = "Evento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int ModelEventId { get; set; }
        [Display(Name = "Evento")]
        public ModelEvent ModelEvent { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Title { get; set; }
    }
}
