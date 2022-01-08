using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("asset_equipment", Schema = "road_asset")]
    public class Equipment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("equipment_alias"), Display(Name = "Alias")]
        public string EquipmentAlias { get; set; }

        [Column("asset_device_id")]
        public int DeviceId { get; set; }
        public Device Device { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(500, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("description"), Display(Name = "Descripción")]
        public string Description { get; set; }

        [Column("asset_group_id"), Display(Name = "Grupo")]
        public int EquipmentGroupId { get; set; }
        [Display(Name = "Grupo")]
        public EquipmentGroup EquipmentGroup { get; set; }

        [Column("last_data_tx"), Display(Name = "Última transmisión"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime LastDataTx { get; set; } = new DateTime();

        [Column("last_latitude")]
        public double? LastLatitude { get; set; }

        [Column("last_longitude")]
        public double? LastLongitude { get; set; }

        [Column("last_position_dt")]
        public DateTime? LastPositionDt { get; set; }

        [Column("notify_info"), Display(Name = "Notificación")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NotifyInfo { get; set; }
    }
}
