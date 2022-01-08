using Datos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LAccount
    {
        private readonly DaoAccount daoAccount;

        public LAccount(VialtecContext context)
        {
            daoAccount = new DaoAccount(context);
        }

        public LAccount() { }

        public async Task<CustomerUser> Login(string email, string passKey)
        {
            passKey = MD5Hash(passKey);
            return await daoAccount.Login(email, passKey);
        }

        public string MD5Hash(string password)
        {
            using (var md5 = MD5.Create())
            {
                //var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
                //return Encoding.ASCII.GetString(result);
                byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
                byte[] hash = md5.ComputeHash(passwordBytes);
                var stringBuilder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    stringBuilder.Append(hash[i].ToString("X2"));
                }
                return stringBuilder.ToString().ToLower();
            }
        }

        public void SendEmailToken(string email, string token)
        {
            MailMessage msg = new MailMessage
            {
                From = new MailAddress("noreply@vialtec.co", "Vialtec"),
                Subject = "Recuperación de contraseña",
                Body = $"<b>Token para la recuperación de contraseña:</b><br/>{token}",
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            msg.To.Add(email);

            SmtpClient client = new SmtpClient
            {
                Host = "vialtec.co",
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("noreply@vialtec.co", "Wit0ut$3zp0nz3"),
                EnableSsl = true
            };
            client.Send(msg);
            return;
        }
    }
}
