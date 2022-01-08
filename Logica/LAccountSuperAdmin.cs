using Datos;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Utilitarios;

namespace Logica
{
    public class LAccountSuperAdmin
    {
        private DaoAccountSuperAdmin daoAccountSuperAdmin;

        public LAccountSuperAdmin(VialtecContext context)
        {
            daoAccountSuperAdmin = new DaoAccountSuperAdmin(context);
        }

        public SAdmin Login(string email, string passKey)
        {
            passKey = MD5Hash(passKey);
            return daoAccountSuperAdmin.Login(email, passKey);
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
    }
}
