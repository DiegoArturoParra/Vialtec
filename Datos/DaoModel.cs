using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoModel
    {
        private readonly VialtecContext _context;

        public DaoModel(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Model> All()
        {
            // Obtener todos los registros Models
            var results = from x in _context.Models
                          select x;
            return results;
        }

        public async Task<int> Create(Model model)
        {
            // Crear nuevo registro Model
            _context.Models.Add(model);
            return await _context.SaveChangesAsync();
        }

        public async Task<Model> Find(int? id)
        {
            // Buscar registro Model por id
            return await _context.Models.FindAsync(id);
        }

        public async Task<int> Update(Model model)
        {
            // Actualizar registro Model
            _context.Models.Update(model);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Model
            var model = await _context.Models.FindAsync(id);
            _context.Models.Remove(model);
            return await _context.SaveChangesAsync();
        }
    }
}
