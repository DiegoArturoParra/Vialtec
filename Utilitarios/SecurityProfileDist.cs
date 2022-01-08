using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("security_profile", Schema = "distributor")]
    public class SecurityProfileDist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("distributor_info_id"), Display(Name = "Distribuidor")]
        public int DistributorInfoId { get; set; }
        [Display(Name = "Distribuidor")]
        public DistributorInfo DistributorInfo { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Display(Name = "Permisos")]
        public ICollection<ProfilePermissionDist> ProfilePermissionsDists { get; set; }
    }
}
