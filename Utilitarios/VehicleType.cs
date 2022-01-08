using System.ComponentModel.DataAnnotations.Schema;

namespace Utilitarios
{
    [Table("vehicle_type", Schema = "reports")]
    public class VehicleType
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [NotMapped]
        public bool Personalized { get; set; }
    }
}
