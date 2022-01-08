using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomerModelEvent
    {
        private readonly VialtecContext _context;

        public DaoCustomerModelEvent(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomerModelEvent> All()
        {
            // Obtener todos los registros Devices
            var results = from x in _context.CustomerModelEvents
                          select x;
            return results;
        }

        public async Task<int> Create(CustomerModelEvent customerModelEvent)
        {
            // Crear nuevo registro CustomerModelEvent
            _context.CustomerModelEvents.Add(customerModelEvent);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomerModelEvent> Find(int? id)
        {
            // Buscar registro CustomerModelEvent por id
            return await _context.CustomerModelEvents.FindAsync(id);
        }

        public async Task<int> Update(CustomerModelEvent customerModelEvent)
        {
            // Actualizar registro CustomerModelEvent
            _context.CustomerModelEvents.Update(customerModelEvent);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerModelEvent
            var customerModelEvent = await _context.CustomerModelEvents.FindAsync(id);
            _context.CustomerModelEvents.Remove(customerModelEvent);
            return await _context.SaveChangesAsync();
        }
    }
}
