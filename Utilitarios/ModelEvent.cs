using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("model_event", Schema = "transmission")]
    public class ModelEvent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("event_id"), Display(Name = "Evento")]
        public int EventId { get; set; }
        [Display(Name = "Evento")]
        public Event Event { get; set; }

        [Column("model_id"), Display(Name = "Modelo")]
        public int ModelId { get; set; }
        [Display(Name = "Modelo")]
        public Model Model { get; set; }
    }
}
