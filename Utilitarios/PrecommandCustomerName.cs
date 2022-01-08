using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("precommand_customer_name", Schema = "command")]
    public class PrecommandCustomerName
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("precommand_id"), Display(Name = "Comando base")]
        public int PrecommandId { get; set; }
        [Display(Name = "Comando base")]
        public Precommand Precommand { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("custom_title"), Display(Name = "Comando cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string CustomerTitle { get; set; }
    }
}
