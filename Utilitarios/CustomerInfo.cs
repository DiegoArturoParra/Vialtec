using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("customer_info", Schema = "customer")]
    public class CustomerInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("title"), Display(Name = "Nombre")]
        public string Title { get; set; }

        [Column("distributor_id"), Display(Name = "Distribuidor")]
        public int DistributorInfoId { get; set; }
        [Display(Name = "Distribuidor")]
        public DistributorInfo DistributorInfo { get; set; }

        [Column("logo_base_64"), Display(Name = "Logo")]
        public string LogoBase64 { get; set; }

        [Column("logo_width")]
        public int LogoWidth { get; set; }

        [Column("logo_height")]
        public int LogoHeight { get; set; }

        [Column("zone_time"), Display(Name = "Zona horaria")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int ZoneTime { get; set; }
    }
}
