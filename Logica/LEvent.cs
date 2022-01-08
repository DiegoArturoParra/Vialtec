using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LEvent
    {
        private readonly DaoEvent daoEvent;

        public LEvent(VialtecContext context)
        {
            daoEvent = new DaoEvent(context);
        }

        public IQueryable<Event> All()
        {
            // Obtener todos los registros Events
            return daoEvent.All();
        }

        public async Task<int> Create(Event @event)
        {
            // Crear nuevo registro Event
            return await daoEvent.Create(@event);
        }

        public async Task<Event> Find(int? id)
        {
            // Buscar registro Event por id
            return await daoEvent.Find(id);
        }

        public async Task<int> Update(Event @event)
        {
            // Actualizar registro Event
            return await daoEvent.Update(@event);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Event
            return await daoEvent.Delete(id);
        }
    }
}
