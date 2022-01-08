using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("profile_permission", Schema = "distributor")]
    public class ProfilePermissionDist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("security_profile_id"), Display(Name = "Perfil Seguridad")]
        public int SecurityProfileDistId { get; set; }
        [Display(Name = "Perfil Seguridad")]
        public SecurityProfileDist SecurityProfileDist { get; set; }

        [Column("single_permission"), Display(Name = "Permiso")]
        public int SinglePermissionDistId { get; set; }
        [Display(Name = "Permiso")]
        public SinglePermissionDist SinglePermissionDist { get; set; }
    }
}
