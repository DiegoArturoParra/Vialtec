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
using Vialtec.Models;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class CustomerEventNotificationsController : Controller
    {
        private readonly VialtecContext _context;
        private LCustomerEventNotification _logicCustomerEventNotification;

        public CustomerEventNotificationsController(VialtecContext context)
        {
            _context = context;
            _logicCustomerEventNotification = new LCustomerEventNotification(context);
        }

        // GET: CustomerEventNotifications
        public async Task<IActionResult> Index(int? pageNumber)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Obtener el customerInfoId de los Claims definidos en la autentificación
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 40;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            //ViewData["nombre"] = nombre;

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = _logicCustomerEventNotification.All()
                         .Include(c => c.CustomerModelEvent).ThenInclude(x => x.ModelEvent).ThenInclude(x => x.Model)
                         .Include(c => c.EmailNotificationProfile)
                         .Include(c => c.TelegramNotificationProfile)
                         .Where(x => x.CustomerInfoId == customerInfoId);

            // Filtro nombre
            //if (!string.IsNullOrEmpty(nombre))
            //{
            //    query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            //}

            // si no hay registros
            if (query.ToList().Count() == 0)
            {
                ViewData["emptyMessage"] = "No se encontraron resultados";
            }

            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convertir por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasar el query de EquipmentGroup por el modelo de paginación
            return View(await PaginatedList<CustomerEventNotification>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: CustomerEventNotifications/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title");
            ViewData["EmailNotificationProfileId"] = _context.EmailNotificationProfiles
                                                    .Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();
            ViewData["TelegramNotificationProfileId"] = _context.TelegramNotificationProfiles
                                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerModelEventsByModelId(int modelId, int? customerModelEventId)
        {
            var results = _context.CustomerModelEvents.Include(x => x.ModelEvent)
                           .Where(x => x.CustomerInfoId == GetCustomerInfoId() && x.ModelEvent.ModelId == modelId);
            return Json(await results.ToListAsync());
        }

        // POST: CustomerEventNotifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerModelEventId,EmailNotificationProfileId,TelegramNotificationProfileId,BodyText")] CustomerEventNotification customerEventNotification)
        {
            customerEventNotification.CustomerInfoId = GetCustomerInfoId();

            await _logicCustomerEventNotification.Create(customerEventNotification);
            return RedirectToAction(nameof(Index));
        }

        // GET: CustomerEventNotifications/Edit/5
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

            var customerEventNotification = await _logicCustomerEventNotification.Find(id);
            if (customerEventNotification == null)
            {
                return NotFound();
            }
            int modelId = _context.CustomerModelEvents.Include(x => x.ModelEvent)
                        .Where(x => x.Id == customerEventNotification.CustomerModelEventId)
                        .Select(x => x.ModelEvent.ModelId).FirstOrDefault();
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", modelId);
            ViewData["EmailNotificationProfileId"] = _context.EmailNotificationProfiles
                                                    .Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();
            ViewData["TelegramNotificationProfileId"] = _context.TelegramNotificationProfiles
                                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();
            return View(customerEventNotification);
        }

        // POST: CustomerEventNotifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,CustomerModelEventId,EmailNotificationProfileId,TelegramNotificationProfileId,BodyText")] CustomerEventNotification customerEventNotification)
        {
            if (id != customerEventNotification.Id)
            {
                return NotFound();
            }
            try
            {
                await _logicCustomerEventNotification.Update(customerEventNotification);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerEventNotificationExists(customerEventNotification.Id))
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

        // GET: CustomerEventNotifications/Delete/5
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

            var customerEventNotification = await _logicCustomerEventNotification.All()
                .Include(c => c.CustomerModelEvent)
                .Include(c => c.EmailNotificationProfile)
                .Include(c => c.TelegramNotificationProfile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customerEventNotification == null)
            {
                return NotFound();
            }

            return View(customerEventNotification);
        }

        // POST: CustomerEventNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicCustomerEventNotification.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerEventNotificationExists(int id)
        {
            return _context.CustomerEventNotifications.Any(e => e.Id == id);
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
