using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoPrecommandByUser
    {
        private readonly VialtecContext _context;

        public DaoPrecommandByUser(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<PrecommandByUser> All()
        {
            // Obtener todos los registros PrecommandByUsers
            var results = from x in _context.PrecommandByUsers
                          select x;
            return results;
        }

        public async Task<int> CreateRange(List<PrecommandByUser> precommandsByUser)
        {
            // Agregar una lista de registros PrecommandByUser
            _context.PrecommandByUsers.AddRange(precommandsByUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<PrecommandByUser> Find(int? id)
        {
            // Buscar registro PrecommandByUser por id
            return await _context.PrecommandByUsers.FindAsync(id);
        }

        public async Task<int> Update(PrecommandByUser precommandByUser)
        {
            // Actualizar registro PrecommandByUser
            _context.PrecommandByUsers.Update(precommandByUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteRange(List<PrecommandByUser> precommandsByUser)
        {
            // Eliminar una lista de registros PrecommandByUser
            _context.PrecommandByUsers.RemoveRange(precommandsByUser);
            return await _context.SaveChangesAsync();
        }
    }
}
