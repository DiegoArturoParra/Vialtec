using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomerUserPermission
    {
        private readonly DaoCustomerUserPermission daoCustomerUserPermission;

        public LCustomerUserPermission(VialtecContext context)
        {
            daoCustomerUserPermission = new DaoCustomerUserPermission(context);
        }

        public IQueryable<CustomerUserPermission> All()
        {
            // Obtener todos los registros CustomerUserPermission
            return daoCustomerUserPermission.All();
        }

        public async Task<int> Create(CustomerUserPermission customerUserPermission)
        {
            // Agregar nuevo registro CustomerUserPermission
            return await daoCustomerUserPermission.Create(customerUserPermission);
        }

        public async Task<int> CreateRange(IEnumerable<CustomerUserPermission> customerUserPermissions)
        {
            // Agregar una lista de CustomerUserPermission
            return await daoCustomerUserPermission.CreateRange(customerUserPermissions);
        }

        public async Task<CustomerUserPermission> Find(int? id)
        {
            // Buscar un CustomerUserPermission por id
            return await daoCustomerUserPermission.Find(id);
        }

        public async Task<int> Update(CustomerUserPermission customerUserPermission)
        {
            // Actualizar un registro CustomerUserPermission
            return await daoCustomerUserPermission.Update(customerUserPermission);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerUserPermission
            return await daoCustomerUserPermission.Delete(id);
        }

        public async Task<int> DeleteRange(IEnumerable<CustomerUserPermission> customerUserPermissions)
        {
            // Eliminar una lista de CustomerUserPermission
            return await daoCustomerUserPermission.DeleteRange(customerUserPermissions);
        }
    }
}
