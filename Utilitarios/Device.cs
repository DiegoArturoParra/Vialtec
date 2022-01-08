using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("asset_device", Schema = "road_asset")]
    public class Device
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("asset_serial"), Display(Name = "Serial")]
        public string AssetSerial { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("network_identifier"), Display(Name = "Identificador Red")]
        public string NetworkIdentifier { get; set; }

        [Column("model_id"), Display(Name = "Modelo")]
        public int ModelId { get; set; }
        [Display(Name = "Modelo")]
        public Model Model { get; set; }

        [Column("creation_dt"), Display(Name = "Fecha creación"), DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime CreationDT { get; set; } = DateTime.Now;

        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int? CustomerInfoId { get; set; }
        [Display(Name = "Info Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("distributor_info_id"), Display(Name = "Distribuidor")]
        public int DistributorInfoId { get; set; }
        [Display(Name = "Distribuidor")]
        public DistributorInfo DistributorInfo { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Column("zone_time"), Display(Name = "Zona horaria")]
        public int? ZoneTime { get; set; }

        [Column("bluetooth_info", TypeName = "json"), Display(Name = "Info Bluetooth")]
        public string BluetoothInfo { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(30, ErrorMessage = "Longitud máxima de {1} caracteres")]
        [Column("dev_pass")]
        public string DevPass { get; set; }
    }
}
