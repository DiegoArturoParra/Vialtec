using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LDistributorInfo
    {
        private DaoDistributorInfo daoDistributorInfo;

        public LDistributorInfo(VialtecContext context)
        {
            daoDistributorInfo = new DaoDistributorInfo(context);
        }

        public IQueryable<DistributorInfo> All()
        {
            return daoDistributorInfo.All();
        }

        public async Task<int> Create(DistributorInfo distributorInfo)
        {
            return await daoDistributorInfo.Create(distributorInfo);
        }

        public async Task<DistributorInfo> Find(int? id)
        {
            return await daoDistributorInfo.Find(id);
        }

        public async Task<int> Update(DistributorInfo distributorInfo)
        {
            return await daoDistributorInfo.Update(distributorInfo);
        }

        public async Task<int> Delete(int id)
        {
            return await daoDistributorInfo.Delete(id);
        }
    }
}
