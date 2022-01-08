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
using Microsoft.AspNetCore.Authorization;
using Vialtec.Models;
using System.Security.Claims;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerModelEventsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCustomerModelEvent _logicCustomerModelEvent;

        public CustomerModelEventsController(VialtecContext context)
        {
            _context = context;
            _logicCustomerModelEvent = new LCustomerModelEvent(context);
        }

        // GET: Admin/CustomerModelEvents
        public async Task<IActionResult> Index(int? pageNumber, string nombre, int? customerInfoId, int? modelId, int? eventId)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Obtener el distributorInfoId de los Claims definidos en la autentificación
            int distributorInfoId = GetDistributorInfoId();

            // Número de registros por página
            int pageSize = 25;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;
            ViewData["modelId"] = modelId;
            ViewData["eventId"] = eventId;
            ViewData["customerInfoId"] = customerInfoId;
            // ViewDatas de los para los select
            ViewData["models"] = _context.Models.ToList();
            ViewData["events"] = _context.Events.ToList();
            ViewData["customersInfo"] = _context.CustomerInfos.Where(x => x.DistributorInfoId == distributorInfoId).ToList();

            // Obtener los customerInfo del distributorInfo
            var customerInfoIDs = _context.CustomerInfos
                                    .Where(x => x.DistributorInfoId == distributorInfoId)
                                    .Select(x => x.Id);

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = _context.CustomerModelEvents.Include(x => x.CustomerInfo)
                        .Where(x => customerInfoIDs.Contains(x.CustomerInfoId));

            // Es necesario incluir por separado porque después de un ThenInclude ya hace referencia a ese nuevo Objeto Include
            query = query.Include(c => c.ModelEvent).ThenInclude(x => x.Model);
            query = query.Include(x => x.ModelEvent).ThenInclude(x => x.Event);

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Filtro de cliente
            if (customerInfoId != null && customerInfoId != -1)
            {
                query = query.Where(x => x.CustomerInfoId == customerInfoId);
            }

            // Filtro de evento
            if (eventId != null && eventId != -1)
            {
                query = query.Where(x => x.ModelEvent.EventId == eventId);
            }

            // Filtro de modelo
            if (modelId != null && modelId != -1)
            {
                query = query.Where(x => x.ModelEvent.ModelId == modelId); 
            }

            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convertir por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasar el query de EquipmentGroup por el modelo de paginación
            return View(await PaginatedList<CustomerModelEvent>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetDistributorInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
        }

        // GET: Admin/CustomerModelEvents/Create
        public async Task<IActionResult> Create()
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["CustomerInfoId"] = new SelectList(
                _context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId()),
                "Id", "Title");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View();
        }

        // POST: Admin/CustomerModelEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerInfoId,ModelEventId,Title")] CustomerModelEvent customerModelEvent)
        {
            if (ModelState.IsValid)
            {
                await _logicCustomerModelEvent.Create(customerModelEvent);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerInfoId"] = new SelectList
                (_context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId()),
                "Id", "Title", customerModelEvent.CustomerInfoId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View(customerModelEvent);
        }

        // GET: Admin/CustomerModelEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var customerModelEvent = await _logicCustomerModelEvent.All()
                                            .Include(x => x.ModelEvent).ThenInclude(x => x.Model)
                                            .FirstOrDefaultAsync(x => x.Id == id);
            if (customerModelEvent == null)
            {
                return NotFound();
            }
            ViewData["modelId"] = customerModelEvent.ModelEvent.ModelId;
            ViewData["CustomerInfoId"] = new SelectList(
                _context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId()), 
                "Id", "Title", customerModelEvent.CustomerInfoId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", customerModelEvent.ModelEvent.Model.CategoryId);
            return View(customerModelEvent);
        }

        // POST: Admin/CustomerModelEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,ModelEventId,Title")] CustomerModelEvent customerModelEvent)
        {
            if (id != customerModelEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicCustomerModelEvent.Update(customerModelEvent);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerModelEventExists(customerModelEvent.Id))
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
            ViewData["CustomerInfoId"] = new SelectList(
                _context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId()), 
                "Id", "Title", customerModelEvent.CustomerInfoId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View(customerModelEvent);
        }

        // GET: Admin/CustomerModelEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var query = _logicCustomerModelEvent.All();
            query = query.Include(x => x.CustomerInfo).Include(x => x.ModelEvent).ThenInclude(x => x.Model);
            query = query.Include(x => x.ModelEvent).ThenInclude(x => x.Event);
            var customerModelEvent = await query.FirstOrDefaultAsync(x => x.Id == id);
            if (customerModelEvent == null)
            {
                return NotFound();
            }

            return View(customerModelEvent);
        }

        // POST: Admin/CustomerModelEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicCustomerModelEvent.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerModelEventExists(int id)
        {
            return _context.CustomerModelEvents.Any(e => e.Id == id);
        }

        /// <summary>
        /// Obtener los modelos con el categoryId recibido como parámetro
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Json de models</returns>
        [HttpGet]
        public async Task<JsonResult> GetModelsByCategoryId(int categoryId)
        {
            var models = _context.Models.Where(x => x.CategoryId == categoryId);
            return Json(await models.ToListAsync());
        }

        /// <summary>
        /// Obtener los ModelEvents para un modelo y cliente recibidos como parámetros
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="customerInfoId"></param>
        /// <returns>Json de ModelEvents</returns>
        [HttpGet]
        public async Task<JsonResult> GetModelEventsByModelAndCustomerInfo(int modelId, int customerInfoId)
        {
            var modelEvents = _context.ModelEvents.Include(x => x.Model).Include(x => x.Event)
                                .Where(x => x.ModelId == modelId);
            // Los modelEvents que ya están siendo utilizados por el cliente en customerModelEvent
            var modelEventsUtilizados = _context.CustomerModelEvents
                                        .Where(x => x.CustomerInfoId == customerInfoId)
                                        .Select(x => x.ModelEvent);
            return Json(await modelEvents.Except(modelEventsUtilizados).ToListAsync());
        }

        /// <summary>
        /// Obtener los ModelEvents para un modelo y cliente recibidos como parámetros
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="modelEventId"></param>
        /// <param name="customerInfoId"></param>
        /// <returns>Json de ModelEvents</returns>
        [HttpGet]
        public async Task<JsonResult> GetModelEventsByModelAndCustomerInfoUpdate(int modelId, int modelEventId, int customerInfoId)
        {
            // Obtener los modelEvents por modelo
            var modelEvents = _context.ModelEvents.Include(x => x.Model).Include(x => x.Event)
                                .Where(x => x.ModelId == modelId);
            // Los modelEvents que ya están siendo utilizado en customerModelEvent y tiene en cuenta el actual modelEventId
            var modelEventsUtilizados = _context.CustomerModelEvents
                                        .Where(x => x.CustomerInfoId == customerInfoId && x.ModelEventId != modelEventId)
                                        .Select(x => x.ModelEvent);
            return Json(await modelEvents.Except(modelEventsUtilizados).ToListAsync());
        }

        /// <summary>
        /// Se encarga de determinar si el distributorUser autenticado tiene acceso a al controlador/vistas
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AccessGranted()
        {
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            int distributorUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorUserId").Value);
            var customerUser = await _context.DistributorUsers
                                .Include(x => x.SecurityProfileDist)
                                .ThenInclude(x => x.ProfilePermissionsDists)
                                .ThenInclude(x => x.SinglePermissionDist)
                                .FirstOrDefaultAsync(x => x.Id == distributorUserId);
            var singlePermissionIdentifiers = customerUser.SecurityProfileDist.ProfilePermissionsDists
                                                .Select(x => x.SinglePermissionDist.Identifier).ToList();
            return singlePermissionIdentifiers.Contains(nameController);
        }
    }
}
