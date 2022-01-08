using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("distributor_info", Schema = "distributor")]
    public class DistributorInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("email"), DataType(DataType.EmailAddress, ErrorMessage = "Formato de correo electrónico no valido")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Email { get; set; }

        [Column("active"), Display(Name = "Estado")]
        public bool Active { get; set; }

        [Column("address"), Display(Name = "Dirección")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Address { get; set; }

        [Column("city"), Display(Name = "Ciudad")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string City { get; set; }

        [Column("country"), Display(Name = "País")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Country { get; set; }

        [Column("contact_person"), Display(Name = "Contacto")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(50, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string ContactPerson { get; set; }
    }
}
