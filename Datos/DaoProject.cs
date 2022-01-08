using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoProject
    {
        private readonly VialtecContext _context;

        public DaoProject(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Project> All()
        {
            // Obtener todos los registros Project
            var results = from c in _context.Projects
                          select c;
            return results;
        }

        public async Task<int> Create(Project project)
        {
            // Crear nuevo registro Project
            _context.Projects.Add(project);
            return await _context.SaveChangesAsync();
        }

        public async Task<Project> Find(int? id)
        {
            // Buscar registro Project
            return await _context.Projects.FindAsync(id);
        }

        public async Task<int> Update(Project project)
        {
            // Actualizar registro project
            _context.Update(project);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro project
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            return await _context.SaveChangesAsync();
        }
    }
}
