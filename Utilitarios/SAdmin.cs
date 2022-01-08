using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("super_admin", Schema = "super_admin")]
    public class SAdmin
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email"), DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Formato de correo electrónico no valido")]
        public string Email { get; set; }

        [Column("password"), DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; }
    }
}
