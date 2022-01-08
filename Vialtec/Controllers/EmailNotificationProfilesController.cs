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

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class EmailNotificationProfilesController : Controller
    {
        private readonly VialtecContext _context;
        private LEmailNotificationProfile _logicEmailNotificationProfile;

        public EmailNotificationProfilesController(VialtecContext context)
        {
            _context = context;
            _logicEmailNotificationProfile = new LEmailNotificationProfile(context);
        }

        // GET: EmailNotificationProfiles
        public async Task<IActionResult> Index()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            int customerInfoId = GetCustomerInfoId();
            var query = _logicEmailNotificationProfile.All().Where(x => x.CustomerInfoId == customerInfoId);
            return View(await query.ToListAsync());
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: EmailNotificationProfiles/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View();
        }

        // POST: EmailNotificationProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,EmailAddress")] EmailNotificationProfile emailNotificationProfile)
        {
            emailNotificationProfile.CustomerInfoId = GetCustomerInfoId();
            emailNotificationProfile.EmailAddress = JsonConvert.SerializeObject(new { email_address = emailNotificationProfile.EmailAddress.Split('#') });
            if (ModelState.IsValid)
            {
                await _logicEmailNotificationProfile.Create(emailNotificationProfile);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", emailNotificationProfile.CustomerInfoId);
            return View(emailNotificationProfile);
        }

        // GET: EmailNotificationProfiles/Edit/5
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

            var emailNotificationProfile = await _logicEmailNotificationProfile.Find(id);
            if (emailNotificationProfile == null)
            {
                return NotFound();
            }
            ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", emailNotificationProfile.CustomerInfoId);
            return View(emailNotificationProfile);
        }

        // POST: EmailNotificationProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,Title,Description,EmailAddress")] EmailNotificationProfile emailNotificationProfile)
        {
            if (id != emailNotificationProfile.Id)
            {
                return NotFound();
            }

            emailNotificationProfile.EmailAddress = JsonConvert.SerializeObject(new { email_address = emailNotificationProfile.EmailAddress.Split('#') });
            if (ModelState.IsValid)
            {
                try
                {
                    await _logicEmailNotificationProfile.Update(emailNotificationProfile);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailNotificationProfileExists(emailNotificationProfile.Id))
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
            ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", emailNotificationProfile.CustomerInfoId);
            return View(emailNotificationProfile);
        }

        // GET: EmailNotificationProfiles/Delete/5
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

            var emailNotificationProfile = await _logicEmailNotificationProfile.All()
                .Include(e => e.CustomerInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Observar las Foreign Keys
            var customerEventNotifications = _context.CustomerEventNotifications
                                             .Where(x => x.EmailNotificationProfileId == id)
                                             .ToList();
            if (customerEventNotifications.Count() != 0)
            {
                ViewData["foreingKeyMessage"] = "El perfil está siendo utilizado en las notificaciones de eventos";
            }

            if (emailNotificationProfile == null)
            {
                return NotFound();
            }

            return View(emailNotificationProfile);
        }

        // POST: EmailNotificationProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicEmailNotificationProfile.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EmailNotificationProfileExists(int id)
        {
            return _context.EmailNotificationProfiles.Any(e => e.Id == id);
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
