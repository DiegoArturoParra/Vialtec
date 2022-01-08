using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoTelegramNotificationProfile
    {
        private readonly VialtecContext _context;

        public DaoTelegramNotificationProfile(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<TelegramNotificationProfile> All()
        {
            // Obtener todos los registros TelegramNotificationProfile
            var results = from x in _context.TelegramNotificationProfiles
                          select x;
            return results;
        }

        public async Task<int> Create(TelegramNotificationProfile telegramNotificationProfile)
        {
            // Crear nuevo registro TelegramNotificationProfile
            _context.TelegramNotificationProfiles.Add(telegramNotificationProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<TelegramNotificationProfile> Find(int? id)
        {
            // Buscar registro TelegramNotificationProfile por id
            return await _context.TelegramNotificationProfiles.FindAsync(id);
        }

        public async Task<int> Update(TelegramNotificationProfile telegramNotificationProfile)
        {
            // Actualizar registro TelegramNotificationProfile
            _context.TelegramNotificationProfiles.Update(telegramNotificationProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro TelegramNotificationProfile
            var telegramNotificationProfile = await _context.TelegramNotificationProfiles.FindAsync(id);
            _context.TelegramNotificationProfiles.Remove(telegramNotificationProfile);
            return await _context.SaveChangesAsync();
        }
    }
}
