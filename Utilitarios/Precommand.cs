using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("precommand", Schema = "command")]
    public class Precommand
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("command_data"), Display(Name = "Command data")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string CommandData { get; set; }

        [Column("expected_ack"), Display(Name = "Respuesta esperada")]
        public string ExpectedAck { get; set; }

        [Column("asset_model_id"), Display(Name = "Modelo")]
        public int ModelId { get; set; }
        [Display(Name = "Modelo")]
        public Model Model { get; set; }
    }
}
