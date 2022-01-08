using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoDistributorUser
    {
        private readonly VialtecContext _context;

        public DaoDistributorUser(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<DistributorUser> All()
        {
            // Obtener los registros DistributorUser
            var results = from c in _context.DistributorUsers
                          select c;
            return results;
        }

        public async Task<int> Create(DistributorUser distributorUser)
        {
            // Agregar nuevo registro distributoUser
            _context.DistributorUsers.Add(distributorUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<DistributorUser> Find(int? id)
        {
            // Buscar registro distributorUser por id
            return await _context.DistributorUsers.FindAsync(id);
        }

        public async Task<int> Update(DistributorUser distributorUser)
        {
            // Actualizar registro distributorUser 
            _context.Update(distributorUser);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro distributorUser
            var distributorUser = await _context.DistributorUsers.FindAsync(id);
            _context.DistributorUsers.Remove(distributorUser);
            return await _context.SaveChangesAsync();
        }
    }
}
