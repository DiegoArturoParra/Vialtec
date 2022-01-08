using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LPrecommandCustomerName
    {
        private readonly DaoPrecommandCustomerName daoPrecommandCustomerName;

        public LPrecommandCustomerName(VialtecContext context)
        {
            daoPrecommandCustomerName = new DaoPrecommandCustomerName(context);
        }

        public IQueryable<PrecommandCustomerName> All()
        {
            return daoPrecommandCustomerName.All();
        }

        public async Task<int> Create(PrecommandCustomerName precommandCustomerName)
        {
            // Crear nuevo registro PrecommandCustomerName
            return await daoPrecommandCustomerName.Create(precommandCustomerName);
        }

        public async Task<PrecommandCustomerName> Find(int? id)
        {
            // Buscar registro PrecommandCustomerName por id
            return await daoPrecommandCustomerName.Find(id);
        }

        public async Task<int> Update(PrecommandCustomerName precommandCustomerName)
        {
            // Actualizar registro PrecommandCustomerName
            return await daoPrecommandCustomerName.Update(precommandCustomerName);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro PrecommandCustomerName
            return await daoPrecommandCustomerName.Delete(id);
        }

        public async Task<bool> RelationshipExists(int precommandId, int customerInfoId)
        {
            return await daoPrecommandCustomerName.RelationshipExists(precommandId, customerInfoId);
        }

        public async Task<bool> RelationshipExistsUpdate(int id, int precommandId, int customerInfoId)
        {
            return await daoPrecommandCustomerName.RelationshipExistsUpdate(id, precommandId, customerInfoId);
        }
    }
}
