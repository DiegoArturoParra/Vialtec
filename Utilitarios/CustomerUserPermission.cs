using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("customer_user_permission", Schema = "customer")]
    public class CustomerUserPermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_user_id")]
        public int CustomerUserId { get; set; }
        public CustomerUser CustomerUser { get; set; }

        [Column("single_permission_id")]
        public int SinglePermissionId { get; set; }
        public SinglePermission SinglePermission { get; set; }
    }
}
