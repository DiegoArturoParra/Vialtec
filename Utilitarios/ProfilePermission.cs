using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("profile_permission", Schema = "customer")]
    public class ProfilePermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("security_profile_id"), Display(Name = "Perfil Seguridad")]
        public int SecurityProfileId { get; set; }
        [Display(Name = "Perfil Seguridad")]
        public SecurityProfile SecurityProfile { get; set; }

        [Column("single_permission"), Display(Name = "Permiso")]
        public int SinglePermissionId { get; set; }
        [Display(Name = "Permiso")]
        public SinglePermission SinglePermission { get; set; }
    }
}
