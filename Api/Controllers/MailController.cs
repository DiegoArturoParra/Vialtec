using System.Net;
using System.Net.Mail;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        [HttpPost]
        [Route("send")]
        public IActionResult SendEmail(Mail mail)
        {
            MailMessage msg = new MailMessage
            {
                From = new MailAddress("notificaciones@via-lab.co", "Vialab"),
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            msg.To.Add(mail.To);

            SmtpClient client = new SmtpClient
            {
                Host = "via-lab.co",
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("notificaciones@via-lab.co", "37$2^h3f5hG("),
                EnableSsl = true
            };
            client.Send(msg);

            return Ok();
        }
    }
}