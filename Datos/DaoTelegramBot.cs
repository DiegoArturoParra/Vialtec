using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoTelegramBot
    {
        private readonly VialtecContext _context;

        public DaoTelegramBot(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<TelegramBot> All()
        {
            // Obtener todos los registros TelegramBots
            var results = from x in _context.TelegramBots
                          select x;
            return results;
        }

        public async Task<int> Create(TelegramBot telegramBot)
        {
            // Crear nuevo registro TelegramBot
            _context.TelegramBots.Add(telegramBot);
            return await _context.SaveChangesAsync();
        }

        public async Task<TelegramBot> Find(int? id)
        {
            // Buscar registro TelegramBot por id
            return await _context.TelegramBots.FindAsync(id);
        }

        public async Task<int> Update(TelegramBot telegramBot)
        {
            // Actualizar registro TelegramBot
            _context.TelegramBots.Update(telegramBot);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro TelegramBot
            var telegramBot = await _context.TelegramBots.FindAsync(id);
            _context.TelegramBots.Remove(telegramBot);
            return await _context.SaveChangesAsync();
        }
    }
}
