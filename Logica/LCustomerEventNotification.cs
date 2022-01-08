using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomerEventNotification
    {
        private DaoCustomerEventNotification daoCustomerEventNotification;

        public LCustomerEventNotification(VialtecContext context)
        {
            daoCustomerEventNotification = new DaoCustomerEventNotification(context);
        }

        public IQueryable<CustomerEventNotification> All()
        {
            return daoCustomerEventNotification.All();
        }

        public async Task<int> Create(CustomerEventNotification customerEventNotification)
        {
            return await daoCustomerEventNotification.Create(customerEventNotification);
        }

        public async Task<CustomerEventNotification> Find(int? id)
        {
            // Buscar registro CustomerEventNotification por id
            return await daoCustomerEventNotification.Find(id);
        }

        public async Task<int> Update(CustomerEventNotification customerEventNotification)
        {
            // Actualizar registro CustomerEventNotification
            return await daoCustomerEventNotification.Update(customerEventNotification);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerEventNotification
            return await daoCustomerEventNotification.Delete(id);
        }
    }
}
