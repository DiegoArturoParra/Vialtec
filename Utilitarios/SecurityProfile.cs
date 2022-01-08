using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("security_profile", Schema = "customer")]
    public class SecurityProfile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("customer_info_id"), Display(Name = "Info Cliente")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Info Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        public string Title { get; set; }

        [Display(Name = "Permisos")]
        public ICollection<ProfilePermission> ProfilePermissions { get; set; }
    }
}
