using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LPrecommand
    {
        private readonly DaoPrecommand daoPrecommand;

        public LPrecommand(VialtecContext context)
        {
            daoPrecommand = new DaoPrecommand(context);
        }

        public IQueryable<Precommand> All()
        {
            // Obtener todos los registros Precommands
            return daoPrecommand.All();
        }

        public async Task<int> Create(Precommand precommand)
        {
            // Crear nuevo registro Precommand
            return await daoPrecommand.Create(precommand);
        }

        public async Task<Precommand> Find(int? id)
        {
            // Buscar registro Precommand por id
            return await daoPrecommand.Find(id);
        }

        public async Task<int> Update(Precommand precommand)
        {
            // Actualizar registro Precommand
            return await daoPrecommand.Update(precommand);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Precommand
            return await daoPrecommand.Delete(id);
        }
    }
}
