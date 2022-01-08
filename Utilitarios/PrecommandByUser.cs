using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("precommand_by_user", Schema = "command")]
    public class PrecommandByUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("precommand_id"), Display(Name = "Precommand")]
        public int PrecommandId { get; set; }
        [Display(Name = "Precommand")]
        public Precommand Precommand { get; set; }

        [Column("customer_user_id"), Display(Name = "Usuario")]
        public int CustomerUserId { get; set; }
        [Display(Name = "Usuario")]
        public CustomerUser CustomerUser { get; set; }
    }
}
