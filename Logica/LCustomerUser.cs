using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomerUser
    {
        private DaoCustomerUser daoCustomerUser;

        public LCustomerUser(VialtecContext context)
        {
            daoCustomerUser = new DaoCustomerUser(context);
        }

        public LCustomerUser() {}

        public IQueryable<CustomerUser> All()
        {
            return daoCustomerUser.All();
        }

        public async Task<int> Create(CustomerUser customerUser)
        {
            customerUser.PassKey = MD5Hash(customerUser.PassKey);
            return await daoCustomerUser.Create(customerUser);
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

        public async Task<CustomerUser> Find(int? id)
        {
            return await daoCustomerUser.Find(id);
        }

        public async Task<CustomerUser> Find(int id)
        {
            return await daoCustomerUser.Find(id);
        }

        public async Task<int> Update(CustomerUser customerUser)
        {
            return await daoCustomerUser.Update(customerUser);
        }

        public async Task<int> Delete(int id)
        {
            return await daoCustomerUser.Delete(id);
        }
    }
}
