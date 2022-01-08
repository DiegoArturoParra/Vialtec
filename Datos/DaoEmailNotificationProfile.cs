using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoEmailNotificationProfile
    {
        private readonly VialtecContext _context;

        public DaoEmailNotificationProfile(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<EmailNotificationProfile> All()
        {
            // Obtener todos los registros EmailNotificationProfiles
            var results = from x in _context.EmailNotificationProfiles
                          select x;
            return results;
        }

        public async Task<int> Create(EmailNotificationProfile emailNotificationProfile)
        {
            // Crear nuevo registro EmailNotificationProfile
            _context.EmailNotificationProfiles.Add(emailNotificationProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<EmailNotificationProfile> Find(int? id)
        {
            // Buscar registro EmailNotificationProfile por id
            return await _context.EmailNotificationProfiles.FindAsync(id);
        }

        public async Task<int> Update(EmailNotificationProfile emailNotificationProfile)
        {
            // Actualizar registro EmailNotificationProfile
            _context.EmailNotificationProfiles.Update(emailNotificationProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro EmailNotificationProfile
            var emailNotificationProfile = await _context.EmailNotificationProfiles.FindAsync(id);
            _context.EmailNotificationProfiles.Remove(emailNotificationProfile);
            return await _context.SaveChangesAsync();
        }
    }
}
