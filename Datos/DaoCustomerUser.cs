using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomerUser
    {
        private readonly VialtecContext _context;

        public DaoCustomerUser(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomerUser> All()
        {
            // Obtener todos los registros customerUser
            var results = from c in _context.CustomerUsers
                          select c;
            return results;
        }

        public async Task<int> Create(CustomerUser customerUser)
        {
            // Agregar nuevo registro customerUser
            _context.CustomerUsers.Add(customerUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomerUser> Find(int? id)
        {
            // Buscar un registro customerUser
            return await _context.CustomerUsers.FindAsync(id);
        }

        public async Task<int> Update(CustomerUser customerUser)
        {
            // Actualizar registro customerUser
            _context.Update(customerUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro customerUser
            var customerUser = await _context.CustomerUsers.FindAsync(id);
            _context.CustomerUsers.Remove(customerUser);
            return await _context.SaveChangesAsync();
        }
    }
}
