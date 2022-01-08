using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomerModelEvent
    {
        private readonly DaoCustomerModelEvent daoCustomerModelEvent;

        public LCustomerModelEvent(VialtecContext context)
        {
            daoCustomerModelEvent = new DaoCustomerModelEvent(context);
        }

        public IQueryable<CustomerModelEvent> All()
        {
            return daoCustomerModelEvent.All();
        }

        public async Task<int> Create(CustomerModelEvent customerModelEvent)
        {
            return await daoCustomerModelEvent.Create(customerModelEvent);
        }

        public async Task<CustomerModelEvent> Find(int? id)
        {
            // Buscar registro CustomerModelEvent por id
            return await daoCustomerModelEvent.Find(id);
        }

        public async Task<int> Update(CustomerModelEvent customerModelEvent)
        {
            // Actualizar registro CustomerModelEvent
            return await daoCustomerModelEvent.Update(customerModelEvent);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerModelEvent
            return await daoCustomerModelEvent.Delete(id);
        }
    }
}
