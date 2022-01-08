using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LSecurityProfileDist
    {
        private readonly DaoSecurityProfileDist daoSecurityProfileDist;

        public LSecurityProfileDist(VialtecContext context)
        {
            daoSecurityProfileDist = new DaoSecurityProfileDist(context);
        }

        public IQueryable<SecurityProfileDist> All()
        {
            return daoSecurityProfileDist.All();
        }

        public async Task<int> Create(SecurityProfileDist securityProfileDist)
        {
            return await daoSecurityProfileDist.Create(securityProfileDist);
        }

        public async Task<SecurityProfileDist> Find(int? id)
        {
            return await daoSecurityProfileDist.Find(id);
        }

        public async Task<int> Update(SecurityProfileDist securityProfileDist)
        {
            return await daoSecurityProfileDist.Update(securityProfileDist);
        }

        public async Task<int> Delete(int id)
        {
            return await daoSecurityProfileDist.Delete(id);
        }
    }
}
