using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoSecurityProfileDist
    {
        private readonly VialtecContext _context;

        public DaoSecurityProfileDist(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<SecurityProfileDist> All()
        {
            // Obtener todos los registros securityProfile del distribuidor
            var results = from c in _context.SecurityProfileDists
                          select c;
            return results;
        }

        public async Task<int> Create(SecurityProfileDist securityProfileDist)
        {
            // Crear nuevo registro SecurityProfile para distribuidores
            _context.SecurityProfileDists.Add(securityProfileDist);
            return await _context.SaveChangesAsync();
        }

        public async Task<SecurityProfileDist> Find(int? id)
        {
            // Buscar nuevo registro SecurityProfiel para distribuidores
            return await _context.SecurityProfileDists.FindAsync(id);
        }

        public async Task<SecurityProfileDist> Find(int id)
        {
            // Buscar nuevo registro SecurityProfiel para distribuidores
            return await _context.SecurityProfileDists.FindAsync(id);
        }

        public async Task<int> Update(SecurityProfileDist securityProfileDist)
        {
            // Actualizar registro securityProfile para distribuidores
            _context.Update(securityProfileDist);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro securityProfile para distribuidores
            var securityProfileDist = await _context.SecurityProfileDists.FindAsync(id);
            _context.SecurityProfileDists.Remove(securityProfileDist);
            return await _context.SaveChangesAsync();
        }
    }
}
