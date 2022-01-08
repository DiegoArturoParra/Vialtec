using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCategory
    {
        private readonly DaoCategory daoCategory;

        public LCategory(VialtecContext context)
        {
            daoCategory = new DaoCategory(context);
        }

        public IQueryable<Category> All()
        {
            // Obtener todos los registros Categories
            return daoCategory.All();
        }

        public async Task<int> Create(Category category)
        {
            // Crear nuevo registro Category
            return await daoCategory.Create(category);
        }

        public async Task<Category> Find(int? id)
        {
            // Buscar registro Category por id
            return await daoCategory.Find(id);
        }

        public async Task<int> Update(Category category)
        {
            // Actualizar registro Category
            return await daoCategory.Update(category);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Category
            return await daoCategory.Delete(id);
        }
    }
}
