using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LSecurityProfile
    {
        private DaoSecurityProfile daoSecurityProfile;

        public LSecurityProfile(VialtecContext context)
        {
            daoSecurityProfile = new DaoSecurityProfile(context);
        }

        public IQueryable<SecurityProfile> All()
        {
            return daoSecurityProfile.All();
        }

        public async Task<int> Create(SecurityProfile securityProfile)
        {
            return await daoSecurityProfile.Create(securityProfile);
        }

        public async Task<SecurityProfile> Find(int? id)
        {
            return await daoSecurityProfile.Find(id);
        }

        public async Task<int> Update(SecurityProfile securityProfile)
        {
            return await daoSecurityProfile.Update(securityProfile);
        }

        public async Task<int> Delete(int id)
        {
            return await daoSecurityProfile.Delete(id);
        }
    }
}
