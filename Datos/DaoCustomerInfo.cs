using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomerInfo
    {
        private readonly VialtecContext _context;

        public DaoCustomerInfo(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomerInfo> All()
        {
            // Obtener todos los registros CustomerInfo
            var results = from c in _context.CustomerInfos
                          select c;
            return results;
        }

        public async Task<int> Create(CustomerInfo customerInfo)
        {
            // Agregar nuevo registro CustomerInfo
            _context.CustomerInfos.Add(customerInfo);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomerInfo> Find(int? id)
        {
            // Buscar un customerInfo por id
            return await _context.CustomerInfos.FindAsync(id);
        }

        public async Task<int> Update(CustomerInfo customerInfo)
        {
            // Actualizar un registro customerInfo
            _context.Update(customerInfo);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro customerInfo
            var customerInfo = await _context.CustomerInfos.FindAsync(id);
            _context.CustomerInfos.Remove(customerInfo);
            return await _context.SaveChangesAsync();
        }
    }
}
