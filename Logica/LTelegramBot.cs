using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LTelegramBot
    {
        private readonly DaoTelegramBot daoTelegramBot;

        public LTelegramBot(VialtecContext context)
        {
            daoTelegramBot = new DaoTelegramBot(context);
        }

        public IQueryable<TelegramBot> All()
        {
            // Obtener todos los registros TelegramBots
            return daoTelegramBot.All();
        }

        public async Task<int> Create(TelegramBot telegramBot)
        {
            // Crear nuevo registro TelegramBot
            return await daoTelegramBot.Create(telegramBot);
        }

        public async Task<TelegramBot> Find(int? id)
        {
            // Buscar registro TelegramBot por id
            return await daoTelegramBot.Find(id);
        }

        public async Task<int> Update(TelegramBot telegramBot)
        {
            // Actualizar registro TelegramBot
            return await daoTelegramBot.Update(telegramBot);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro TelegramBot
            return await daoTelegramBot.Delete(id);
        }
    }
}
