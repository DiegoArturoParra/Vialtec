using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Microsoft.AspNetCore.Authorization;
using Logica;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class TelegramBotsController : Controller
    {
        private readonly VialtecContext _context;
        private LTelegramBot _logicTelegramBot;

        public TelegramBotsController(VialtecContext context)
        {
            _context = context;
            _logicTelegramBot = new LTelegramBot(context);
        }

        // GET: SuperAdmin/TelegramBots
        public async Task<IActionResult> Index()
        {
            var query = _logicTelegramBot.All();
            return View(await query.ToListAsync());
        }

        // GET: SuperAdmin/TelegramBots/Create
        public IActionResult Create()
        {
            var query = _logicTelegramBot.All();
            // Solo puede existir un telegram bot
            if (query.ToList().Count() == 0)
            {
                return View();
            } else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: SuperAdmin/TelegramBots/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,AccessToken")] TelegramBot telegramBot)
        {
            telegramBot.AccessToken = telegramBot.AccessToken.Trim();
            if (ModelState.IsValid)
            {
                await _logicTelegramBot.Create(telegramBot);
                return RedirectToAction(nameof(Index));
            }
            return View(telegramBot);
        }

        // GET: SuperAdmin/TelegramBots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var telegramBot = await _logicTelegramBot.Find(id);
            if (telegramBot == null)
            {
                return NotFound();
            }
            return View(telegramBot);
        }

        // POST: SuperAdmin/TelegramBots/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,AccessToken")] TelegramBot telegramBot)
        {
            if (id != telegramBot.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    telegramBot.AccessToken = telegramBot.AccessToken.Trim();
                    await _logicTelegramBot.Update(telegramBot);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TelegramBotExists(telegramBot.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(telegramBot);
        }

        // GET: SuperAdmin/TelegramBots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var telegramBot = await _logicTelegramBot.All()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (telegramBot == null)
            {
                return NotFound();
            }

            return View(telegramBot);
        }

        // POST: SuperAdmin/TelegramBots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicTelegramBot.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TelegramBotExists(int id)
        {
            return _context.TelegramBots.Any(e => e.Id == id);
        }
    }
}
