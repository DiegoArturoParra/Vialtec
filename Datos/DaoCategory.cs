using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCategory
    {
        private readonly VialtecContext _context;

        public DaoCategory(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Category> All()
        {
            // Obtener todos los registros Categories
            var results = from x in _context.Categories
                          select x;
            return results;
        }

        public async Task<int> Create(Category category)
        {
            // Crear nuevo registro Category
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync();
        }

        public async Task<Category> Find(int? id)
        {
            // Buscar registro Category por id
            return await _context.Categories.FindAsync(id);
        }

        public async Task<int> Update(Category category)
        {
            // Actualizar registro Category
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Category
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync();
        }
    }
}
