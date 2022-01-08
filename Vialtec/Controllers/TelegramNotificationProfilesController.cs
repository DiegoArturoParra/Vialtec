using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Logica;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class TelegramNotificationProfilesController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LTelegramNotificationProfile _logicTelegramNotificationProfile;

        public TelegramNotificationProfilesController(VialtecContext context)
        {
            _context = context;
            _logicTelegramNotificationProfile = new LTelegramNotificationProfile(context);
        }

        // GET: TelegramNotificationProfiles
        public async Task<IActionResult> Index()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var telegramBot = _context.TelegramBots.FirstOrDefault();
            if (telegramBot != null)
            {
                ViewData["bot-access-token"] = telegramBot.AccessToken;
            }
            var query = _logicTelegramNotificationProfile.All().Where(x => x.CustomerInfoId == GetCustomerInfoId());
            return View(await query.ToListAsync());
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: TelegramNotificationProfiles/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View();
        }

        // POST: TelegramNotificationProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ChatIDS")] TelegramNotificationProfile telegramNotificationProfile)
        {
            telegramNotificationProfile.CustomerInfoId = GetCustomerInfoId();
            telegramNotificationProfile.ChatIDS = JsonConvert.SerializeObject(new { telegram_chat_ids = telegramNotificationProfile.ChatIDS.Split('#') });
            if (ModelState.IsValid)
            {
                await _logicTelegramNotificationProfile.Create(telegramNotificationProfile);
                return RedirectToAction(nameof(Index));
            }
            return View(telegramNotificationProfile);
        }

        // GET: TelegramNotificationProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var telegramNotificationProfile = await _logicTelegramNotificationProfile.Find(id);
            if (telegramNotificationProfile == null)
            {
                return NotFound();
            }
            return View(telegramNotificationProfile);
        }

        // POST: TelegramNotificationProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,Title,Description,ChatIDS")] TelegramNotificationProfile telegramNotificationProfile)
        {
            if (id != telegramNotificationProfile.Id)
            {
                return NotFound();
            }

            telegramNotificationProfile.ChatIDS = JsonConvert.SerializeObject(new { telegram_chat_ids = telegramNotificationProfile.ChatIDS.Split('#') });
            if (ModelState.IsValid)
            {
                try
                {
                    await _logicTelegramNotificationProfile.Update(telegramNotificationProfile);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TelegramNotificationProfileExists(telegramNotificationProfile.Id))
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
            return View(telegramNotificationProfile);
        }

        // GET: TelegramNotificationProfiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var telegramNotificationProfile = await _logicTelegramNotificationProfile.All()
                .Include(t => t.CustomerInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            //Revisar Foreign Keys 
            var customerEventNotifications = _context.CustomerEventNotifications
                                                .Where(x => x.TelegramNotificationProfileId == id).ToList();
            if (customerEventNotifications.Count() != 0)
            {
                ViewData["foreingKeyMessage"] = "El perfil está siendo utilizado en las notificaciones de eventos";
            }

            if (telegramNotificationProfile == null)
            {
                return NotFound();
            }

            return View(telegramNotificationProfile);
        }

        // POST: TelegramNotificationProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicTelegramNotificationProfile.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TelegramNotificationProfileExists(int id)
        {
            return _context.TelegramNotificationProfiles.Any(e => e.Id == id);
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains("AlarmasEventos");
        }
    }
}
