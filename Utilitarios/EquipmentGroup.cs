using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("equipment_group", Schema = "road_asset")]
    public class EquipmentGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_info_id"), Display(Name = "Info Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Info Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("title"), Display(Name = "Nombre")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(500, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("description"), Display(Name = "Descripción")]
        public string Description { get; set; }
    }
}
