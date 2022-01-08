using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("distributor_user", Schema = "distributor")]
    public class DistributorUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email"), DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Formato de correo electrónico no valido")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), MinLength(5, ErrorMessage = "La {0} debe tener mínimo {1} caracteres")]
        [Column("pass_key"), Display(Name = "Contraseña"), DataType(DataType.Password)]
        public string PassKey { get; set; }

        [Column("security_profile_id"), Display(Name = "Perfil Seguridad")]
        public int SecurityProfileDistId { get; set; }
        [Display(Name = "Perfil Seguridad")]
        public SecurityProfileDist SecurityProfileDist { get; set; }

        [Column("distributor_info_id"), Display(Name = "Distribuidor")]
        public int DistributorInfoId { get; set; }
        [Display(Name = "Distribuidor")]
        public DistributorInfo DistributorInfo { get; set; }
    }
}
