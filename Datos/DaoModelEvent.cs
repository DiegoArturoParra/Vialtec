using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoModelEvent
    {
        private readonly VialtecContext _context;

        public DaoModelEvent(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<ModelEvent> All()
        {
            // Obtener todos los registros ModelEvents
            var results = from x in _context.ModelEvents
                          select x;
            return results;
        }

        public async Task<int> Create(ModelEvent modelEvent)
        {
            // Crear nuevo registro ModelEvent
            _context.ModelEvents.Add(modelEvent);
            return await _context.SaveChangesAsync();
        }

        public bool VerifyRelationshipExists(int modelId, int eventId)
        {
            var record = _context.ModelEvents
                        .Where(x => x.ModelId == modelId && x.EventId == eventId)
                        .FirstOrDefault();
            return record != null;
        }

        public bool VerifyRelationshipExistsUpdate(int id, int modelId, int eventId)
        {
            var record = _context.ModelEvents
                        .Where(x => x.Id != id && x.ModelId == modelId && x.EventId == eventId)
                        .FirstOrDefault();
            return record != null;
        }

        public async Task<ModelEvent> Find(int? id)
        {
            // Buscar registro ModelEvent por id
            return await _context.ModelEvents.FindAsync(id);
        }

        public async Task<int> Update(ModelEvent modelEvent)
        {
            // Actualizar registro ModelEvent
            _context.ModelEvents.Update(modelEvent);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro ModelEvent
            var modelEvent = await _context.ModelEvents.FindAsync(id);
            _context.ModelEvents.Remove(modelEvent);
            return await _context.SaveChangesAsync();
        }
    }
}
