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
    public class LDistributorUser
    {
        private DaoDistributorUser daoDistributorUser;

        public LDistributorUser(VialtecContext context)
        {
            daoDistributorUser = new DaoDistributorUser(context);
        }

        public LDistributorUser() { }

        public IQueryable<DistributorUser> All()
        {
            return daoDistributorUser.All();
        }

        public async Task<int> Create(DistributorUser distributorUser)
        {
            distributorUser.PassKey = MD5Hash(distributorUser.PassKey);
            return await daoDistributorUser.Create(distributorUser);
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

        public async Task<DistributorUser> Find(int? id)
        {
            return await daoDistributorUser.Find(id);
        }

        public async Task<int> Update(DistributorUser distributorUser)
        {
            return await daoDistributorUser.Update(distributorUser);
        }

        public async Task<int> Delete(int id)
        {
            return await daoDistributorUser.Delete(id);
        }
    }
}
