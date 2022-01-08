using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LModel
    {
        private readonly DaoModel daoModel;

        public LModel(VialtecContext context)
        {
            daoModel = new DaoModel(context);
        }

        public IQueryable<Model> All()
        {
            // Obtener todos los registros Models
            return daoModel.All();
        }

        public async Task<int> Create(Model model)
        {
            // Crear nuevo registro Model
            return await daoModel.Create(model);
        }

        public async Task<Model> Find(int? id)
        {
            // Buscar registro Model por id
            return await daoModel.Find(id);
        }

        public async Task<int> Update(Model model)
        {
            // Actualizar registro Model
            return await daoModel.Update(model);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Model
            return await daoModel.Delete(id);
        }
    }
}
