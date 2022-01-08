using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoPrecommand
    {
        private readonly VialtecContext _context;

        public DaoPrecommand(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Precommand> All()
        {
            // Obtener todos los registros Precommands
            var results = from x in _context.Precommands
                          select x;
            return results;
        }

        public async Task<int> Create(Precommand precommand)
        {
            // Crear nuevo registro Precommand
            _context.Precommands.Add(precommand);
            return await _context.SaveChangesAsync();
        }

        public async Task<Precommand> Find(int? id)
        {
            // Buscar registro Precommand por id
            return await _context.Precommands.FindAsync(id);
        }

        public async Task<int> Update(Precommand precommand)
        {
            // Actualizar registro Precommand
            _context.Precommands.Update(precommand);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro Precommand
            var precommand = await _context.Precommands.FindAsync(id);
            _context.Precommands.Remove(precommand);
            return await _context.SaveChangesAsync();
        }
    }
}
