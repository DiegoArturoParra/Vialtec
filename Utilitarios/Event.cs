using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("event", Schema = "transmission")]
    public class Event
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Evento")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("keystr"), Display(Name = "Key")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string KeyStr { get; set; }
    }
}
