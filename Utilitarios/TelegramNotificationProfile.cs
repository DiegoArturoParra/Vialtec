using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("telegram_notification_profile", Schema = "customer")]
    public class TelegramNotificationProfile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        public int CustomerInfoId { get; set; }
        public CustomerInfo CustomerInfo { get; set; }

        [Column("title"), Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Title { get; set; }

        [Column("description"), Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(100, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string Description { get; set; }

        [Column("telegram_chat_ids"), Display(Name = "Chats IDs Telegram")]
        public string ChatIDS { get; set; }
    }
}
