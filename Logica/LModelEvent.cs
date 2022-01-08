using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LModelEvent
    {
        private readonly DaoModelEvent daoModelEvent;

        public LModelEvent(VialtecContext context)
        {
            daoModelEvent = new DaoModelEvent(context);
        }

        public IQueryable<ModelEvent> All()
        {
            // Obtener todos los registros ModelEvents
            return daoModelEvent.All();
        }

        public async Task<int> Create(ModelEvent modelEvent)
        {
            // Crear nuevo registro ModelEvent
            return await daoModelEvent.Create(modelEvent);
        }

        public bool VerifyRelationshipExists(int modelId, int eventId)
        {
            return daoModelEvent.VerifyRelationshipExists(modelId, eventId);
        }

        public bool VerifyRelationshipExistsUpdate(int id, int modelId, int eventId)
        {
            return daoModelEvent.VerifyRelationshipExistsUpdate(id, modelId, eventId);
        }

        public async Task<ModelEvent> Find(int? id)
        {
            // Buscar registro ModelEvent por id
            return await daoModelEvent.Find(id);
        }

        public async Task<int> Update(ModelEvent modelEvent)
        {
            // Actualizar registro ModelEvent
            return await daoModelEvent.Update(modelEvent);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro ModelEvent
            return await daoModelEvent.Delete(id);
        }
    }
}
