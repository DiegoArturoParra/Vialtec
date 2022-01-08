using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("single_permission", Schema = "customer")]
    public class SinglePermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("identifier")]
        public string Identifier { get; set; }

        public ICollection<ProfilePermission> ProfilePermissions { get; set; }
    }
}
