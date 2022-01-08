using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("command", Schema = "transmission")]
    public class Command
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("custom_id")]
        public string CustomId { get; set; }

        [Column("device_identifier")]
        public string DeviceIdentifier { get; set; }

        [Column("command_data")]
        public string CommandData { get; set; }

        [Column("encoding_type_id")]
        public int EncodingTypeId { get; set; }
        public EncodingType EncodingType { get; set; }

        [Column("expected_ack")]
        public string ExpectedAck { get; set; }

        [Column("delivery_state")]
        public int DeliveryState { get; set; }

        [Column("creation_dt")]
        public DateTime CreationDT { get; set; }

        [Column("received_info")]
        public string ReceivedInfo { get; set; }
    }
}
