using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LEmailNotificationProfile
    {
        private DaoEmailNotificationProfile daoEmailNotificationProfile;

        public LEmailNotificationProfile(VialtecContext context)
        {
            daoEmailNotificationProfile = new DaoEmailNotificationProfile(context);
        }

        public IQueryable<EmailNotificationProfile> All()
        {
            return daoEmailNotificationProfile.All();
        }

        public async Task<int> Create(EmailNotificationProfile emailNotificationProfile)
        {
            return await daoEmailNotificationProfile.Create(emailNotificationProfile);
        }

        public async Task<EmailNotificationProfile> Find(int? id)
        {
            // Buscar registro EmailNotificationProfile por id
            return await daoEmailNotificationProfile.Find(id);
        }

        public async Task<int> Update(EmailNotificationProfile emailNotificationProfile)
        {
            // Actualizar registro EmailNotificationProfile
            return await daoEmailNotificationProfile.Update(emailNotificationProfile);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro EmailNotificationProfile
            return await daoEmailNotificationProfile.Delete(id);
        }
    }
}
