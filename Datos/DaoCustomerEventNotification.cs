using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomerEventNotification
    {
        private readonly VialtecContext _context;

        public DaoCustomerEventNotification(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomerEventNotification> All()
        {
            // Obtener todos los registros Devices
            var results = from x in _context.CustomerEventNotifications
                          select x;
            return results;
        }

        public async Task<int> Create(CustomerEventNotification customerEventNotification)
        {
            // Crear nuevo registro CustomerEventNotification
            _context.CustomerEventNotifications.Add(customerEventNotification);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomerEventNotification> Find(int? id)
        {
            // Buscar registro CustomerEventNotification por id
            return await _context.CustomerEventNotifications.FindAsync(id);
        }

        public async Task<int> Update(CustomerEventNotification customerEventNotification)
        {
            // Actualizar registro CustomerEventNotification
            _context.CustomerEventNotifications.Update(customerEventNotification);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomerEventNotification
            var customerEventNotification = await _context.CustomerEventNotifications.FindAsync(id);
            _context.CustomerEventNotifications.Remove(customerEventNotification);
            return await _context.SaveChangesAsync();
        }
    }
}
