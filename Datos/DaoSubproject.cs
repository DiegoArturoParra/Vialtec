using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoSubproject
    {
        private readonly VialtecContext _context;

        public DaoSubproject(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Subproject> All()
        {
            // Obtener todos los registros Subproject(actividades)
            var results = from c in _context.Subprojects
                          select c;
            return results;
        }

        public async Task<int> Create(Subproject subproject)
        {
            // Crear nuevo registro Subproject(actividad)
            _context.Subprojects.Add(subproject);
            return await _context.SaveChangesAsync();
        }

        public async Task<Subproject> Find(int? id)
        {
            // Buscar nuevo registro Subproject(actividad)
            return await _context.Subprojects.FindAsync(id);
        }

        public async Task<int> Update(Subproject subproject)
        {
            // Actualizar registro Subproject(actividad)
            _context.Update(subproject);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro Subproject(actividad)
            var subproject = await _context.Subprojects.FindAsync(id);
            _context.Subprojects.Remove(subproject);
            return await _context.SaveChangesAsync();
        }
    }
}
