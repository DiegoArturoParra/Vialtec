using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomerInfo
    {
        private DaoCustomerInfo daoCustomerInfo;

        public LCustomerInfo(VialtecContext context)
        {
            daoCustomerInfo = new DaoCustomerInfo(context);
        }

        public IQueryable<CustomerInfo> All()
        {
            return daoCustomerInfo.All();
        }

        public async Task<int> Create(CustomerInfo customerInfo)
        {
            return await daoCustomerInfo.Create(customerInfo);
        }

        public async Task<CustomerInfo> Find(int? id)
        {
            return await daoCustomerInfo.Find(id);
        }

        public async Task<int> Update(CustomerInfo customerInfo)
        {
            return await daoCustomerInfo.Update(customerInfo);
        }

        public async Task<int> Delete(int id)
        {
            return await daoCustomerInfo.Delete(id);
        }
    }
}
