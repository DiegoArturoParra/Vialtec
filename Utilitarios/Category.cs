using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("asset_category", Schema = "road_asset")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }
    }
}
