using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoEvent
    {
        private readonly VialtecContext _context;

        public DaoEvent(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Event> All()
        {
            // Obtener todos los registros Events
            var results = from x in _context.Events
                          select x;
            return results;
        }

        public async Task<int> Create(Event @event)
        {
            // Crear nuevo registro Event
            _context.Events.Add(@event);
            return await _context.SaveChangesAsync();
        }

        public async Task<Event> Find(int? id)
        {
            // Buscar registro Event por id
            return await _context.Events.FindAsync(id);
        }

        public async Task<int> Update(Event @event)
        {
            // Actualizar registro Event
            _context.Events.Update(@event);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Event
            var @event = await _context.Events.FindAsync(id);
            _context.Events.Remove(@event);
            return await _context.SaveChangesAsync();
        }
    }
}
