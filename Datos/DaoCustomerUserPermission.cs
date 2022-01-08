using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomerUserPermission
    {
        private readonly VialtecContext _context;

        public DaoCustomerUserPermission(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomerUserPermission> All()
        {
            // Obtener todos los registros CustomerUserPermission
            var results = from c in _context.CustomerUserPermissions
                          select c;
            return results;
        }

        public async Task<int> Create(CustomerUserPermission customerUserPermission)
        {
            // Agregar nuevo registro CustomerUserPermission
            _context.CustomerUserPermissions.Add(customerUserPermission);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CreateRange(IEnumerable<CustomerUserPermission> customerUserPermissions)
        {
            // Agregar una lista de CustomerUserPermission
            _context.CustomerUserPermissions.AddRange(customerUserPermissions);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomerUserPermission> Find(int? id)
        {
            // Buscar un CustomerUserPermission por id
            return await _context.CustomerUserPermissions.FindAsync(id);
        }

        public async Task<int> Update(CustomerUserPermission customerUserPermission)
        {
            // Actualizar un registro CustomerUserPermission
            _context.Update(customerUserPermission);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerUserPermission
            var customerUserPermission = await _context.CustomerUserPermissions.FindAsync(id);
            _context.CustomerUserPermissions.Remove(customerUserPermission);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteRange(IEnumerable<CustomerUserPermission> customerUserPermissions)
        {
            // Eliminar una lista de CustomerUserPermission
            _context.CustomerUserPermissions.RemoveRange(customerUserPermissions);
            return await _context.SaveChangesAsync();
        }
    }
}
