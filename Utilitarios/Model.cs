using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("asset_model", Schema = "road_asset")]
    public class Model
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("asset_category_id"), Display(Name = "Categoria")]
        public int CategoryId { get; set; }
        [Display(Name = "Categoría")]
        public Category Category { get; set; }

        [Column("encoding_type_id"), Display(Name = "Tipo Codificación")]
        public int EncodingTypeId { get; set; }
        [Display(Name = "Tipo Codificación")]
        public EncodingType EncodingType { get; set; }
    }
}
