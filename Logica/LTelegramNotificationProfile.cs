using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LTelegramNotificationProfile
    {
        private readonly DaoTelegramNotificationProfile daoTelegramNotificationProfile;

        public LTelegramNotificationProfile(VialtecContext context)
        {
            daoTelegramNotificationProfile = new DaoTelegramNotificationProfile(context);
        }

        public IQueryable<TelegramNotificationProfile> All()
        {
            // Obtener todos los registros TelegramNotificationProfile
            return daoTelegramNotificationProfile.All();
        }

        public async Task<int> Create(TelegramNotificationProfile telegramNotificationProfile)
        {
            // Crear nuevo registro TelegramNotificationProfile
            return await daoTelegramNotificationProfile.Create(telegramNotificationProfile);
        }

        public async Task<TelegramNotificationProfile> Find(int? id)
        {
            // Buscar registro TelegramNotificationProfile por id
            return await daoTelegramNotificationProfile.Find(id);
        }

        public async Task<int> Update(TelegramNotificationProfile telegramNotificationProfile)
        {
            // Actualizar registro TelegramNotificationProfile
            return await daoTelegramNotificationProfile.Update(telegramNotificationProfile);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro TelegramNotificationProfile
            return await daoTelegramNotificationProfile.Delete(id);
        }
    }
}
