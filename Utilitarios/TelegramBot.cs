using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("telegram_bots", Schema = "super_admin")]
    public class TelegramBot
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("access_token"), Display(Name = "Token de acceso")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string AccessToken { get; set; }
    }
}
