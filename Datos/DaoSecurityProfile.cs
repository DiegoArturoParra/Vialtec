using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoSecurityProfile
    {
        private readonly VialtecContext _context;

        public DaoSecurityProfile(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<SecurityProfile> All()
        {
            // Obtener todos los registros SecurityProfile
            var results = from c in _context.SecurityProfiles
                          select c;
            return results;
        }

        public async Task<int> Create(SecurityProfile securityProfile)
        {
            // Crear nuevo registro securityProfile
            _context.SecurityProfiles.Add(securityProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<SecurityProfile> Find(int? id)
        {
            // Buscar registro securityProfile
            return await _context.SecurityProfiles.FindAsync(id);
        }

        public async Task<SecurityProfile> Find(int id)
        {
            // Buscar registro securityProfile
            return await _context.SecurityProfiles.FindAsync(id);
        }

        public async Task<int> Update(SecurityProfile securityProfile)
        {
            // Actualizar registro securityProfile
            _context.Update(securityProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro securityProfile
            var securityProfile = await _context.SecurityProfiles.FindAsync(id);
            _context.SecurityProfiles.Remove(securityProfile);
            return await _context.SaveChangesAsync();
        }
    }
}
