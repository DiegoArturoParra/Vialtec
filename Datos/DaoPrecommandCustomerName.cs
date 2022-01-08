using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoPrecommandCustomerName
    {
        private readonly VialtecContext _context;

        public DaoPrecommandCustomerName(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<PrecommandCustomerName> All()
        {
            // Obtener todos los registros PrecommandCustomerNames
            var results = from x in _context.PrecommandCustomerNames
                          select x;
            return results;
        }

        public async Task<int> Create(PrecommandCustomerName precommandCustomerName)
        {
            // Crear nuevo registro PrecommandCustomerName
            _context.PrecommandCustomerNames.Add(precommandCustomerName);
            return await _context.SaveChangesAsync();
        }

        public async Task<PrecommandCustomerName> Find(int? id)
        {
            // Buscar registro PrecommandCustomerName por id
            return await _context.PrecommandCustomerNames.FindAsync(id);
        }

        public async Task<int> Update(PrecommandCustomerName precommandCustomerName)
        {
            // Actualizar registro PrecommandCustomerName
            _context.PrecommandCustomerNames.Update(precommandCustomerName);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro PrecommandCustomerName
            var precommandCustomerName = await _context.PrecommandCustomerNames.FindAsync(id);
            _context.PrecommandCustomerNames.Remove(precommandCustomerName);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> RelationshipExists(int precommandId, int customerInfoId)
        {
            var precommandCustomerName = await _context.PrecommandCustomerNames
                                                .Where(x => x.PrecommandId == precommandId && x.CustomerInfoId == customerInfoId)
                                                .FirstOrDefaultAsync();
            return precommandCustomerName != null;
        }

        public async Task<bool> RelationshipExistsUpdate(int id, int precommandId, int customerInfoId)
        {
            var precommandCustomerName = await _context.PrecommandCustomerNames
                                                .Where(x => x.Id != id && x.PrecommandId == precommandId && x.CustomerInfoId == customerInfoId)
                                                .FirstOrDefaultAsync();
            return precommandCustomerName != null;
        }
    }
}
