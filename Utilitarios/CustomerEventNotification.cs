using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("customer_event_notification", Schema = "customer")]
    public class CustomerEventNotification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_info_id"), Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int CustomerInfoId { get; set; }
        [Display(Name = "Cliente")]
        public CustomerInfo CustomerInfo { get; set; }

        [Column("customer_model_event_id"), Display(Name = "Evento de equipo")]
        [Required(ErrorMessage = "No hay eventos de equipos")]
        public int CustomerModelEventId { get; set; }
        [Display(Name = "Evento de equipo")]
        public CustomerModelEvent CustomerModelEvent { get; set; }

        [Column("email_notification_profile_id"), Display(Name = "Perfil de notificación email")]
        public int? EmailNotificationProfileId { get; set; }
        [Display(Name = "Perfil de notificación email")]
        public EmailNotificationProfile EmailNotificationProfile { get; set; }

        [Column("telegram_notification_profile_id"), Display(Name = "Perfil de notificación telegram")]
        public int? TelegramNotificationProfileId { get; set; }
        [Display(Name = "Perfil de notificación telegram")]
        public TelegramNotificationProfile TelegramNotificationProfile { get; set; }

        [Column("body_text"), Display(Name = "Mensaje")]
        [Required(ErrorMessage = "El campo {0} es requerido"), StringLength(200, ErrorMessage = "Longitud máxima de {1} caracteres")]
        public string BodyText { get; set; }
    }
}
