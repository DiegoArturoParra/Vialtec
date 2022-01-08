using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoDistributorInfo
    {
        private readonly VialtecContext _context;

        public DaoDistributorInfo(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<DistributorInfo> All()
        {
            // Obtener todos los registros DistributoInfo
            var results = from c in _context.DistributorInfos
                          select c;
            return results;
        }

        public async Task<int> Create(DistributorInfo distributorInfo)
        {
            // Crear nuevo registro distributorInfo
            _context.DistributorInfos.Add(distributorInfo);
            return await _context.SaveChangesAsync();
        }

        public async Task<DistributorInfo> Find(int? id)
        {
            // Buscar registro distributoInfo por id
            return await _context.DistributorInfos.FindAsync(id);
        }

        public async Task<int> Update(DistributorInfo distributorInfo)
        {
            // Actualizar registro distributorInfo 
            _context.Update(distributorInfo);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro distributorInfo
            var distributorInfo = await _context.DistributorInfos.FindAsync(id);
            _context.DistributorInfos.Remove(distributorInfo);
            return await _context.SaveChangesAsync();
        }
    }
}
