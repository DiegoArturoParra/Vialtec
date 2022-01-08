using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("customer_user", Schema = "customer")]
    public class CustomerUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("user_alias"), Display(Name = "Email"), DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Formato de correo electrónico no valido")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Email { get; set; }

        [Column("pass_key"), Display(Name = "Contraseña"), DataType(DataType.Password)]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [MinLength(5, ErrorMessage = "La {0} debe tener mínimo {1} caracteres")]
        public string PassKey { get; set; }

        [Column("security_profile_id"), Display(Name = "Perfil Seguridad")]
        public int SecurityProfileId { get; set; }
        [Display(Name = "Perfil Seguridad")]
        public SecurityProfile SecurityProfile { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("is_root"), Display(Name = "Rol")]
        public bool IsRoot { get; set; }
    }
}
