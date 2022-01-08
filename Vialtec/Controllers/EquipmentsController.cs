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
    public class EquipmentsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LEquipment logicEquipment;

        public EquipmentsController(VialtecContext context)
        {
            _context = context;
            logicEquipment = new LEquipment(context);
        }

        // GET: Equipments
        public async Task<IActionResult> Index(int? pageNumber, int? equipmentGroupId, int? categoryId, int? modelId, string alias, string serial)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el customerInfoId de los Claims del login
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Número de páginas que serán generadas
            int totalPages = 0;

            // ViewBags para los filtros del index
            ViewBag.equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id).ToList();
            ViewBag.categories = _context.Categories.OrderBy(x => x.Id).ToList();
            ViewBag.models = _context.Models.OrderBy(x => x.Id).ToList();

            // ViewDatas para los filtros
            ViewData["equipmentGroupId"] = equipmentGroupId;
            ViewData["categoryId"] = categoryId;
            ViewData["modelId"] = modelId;
            ViewData["alias"] = alias;
            ViewData["serial"] = serial;
            // Obtener el zonetime del cliente para manejar el tema de la última transmissión del equipment
            var customerInfo = await _context.CustomerInfos
                                    .Where(x => x.Id == GetCustomerInfoId())
                                    .FirstOrDefaultAsync();
            ViewData["zoneTime"] = customerInfo.ZoneTime;

            // Consulta de Equipment por customerInfoId
            var query = logicEquipment.All()
                        .Include(e => e.Device).ThenInclude(z => z.Model).ThenInclude(x => x.Category)
                        .Include(e => e.EquipmentGroup)
                        .Where(x => x.EquipmentGroup.CustomerInfoId == customerInfoId);

            // APLICAR FILTROS

            // Filtro de equipmentGroupId
            if (equipmentGroupId != null && equipmentGroupId != -1)
            {
                query = query.Where(x => x.EquipmentGroupId.Equals(equipmentGroupId));
            }

            // Filtro categoryId
            if (categoryId != null && categoryId != -1)
            {
                query = query.Where(x => x.Device.Model.CategoryId.Equals(categoryId));
            }

            // Filtro modelId
            if (modelId != null && modelId != -1)
            {
                query = query.Where(x => x.Device.ModelId.Equals(modelId));
            }

            // Filtro Alias
            if (!string.IsNullOrEmpty(alias))
            {
                query = query.Where(x => x.EquipmentAlias.ToLower().Contains(alias.ToLower()));
            }

            // Filtro Serial
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(x => x.Device.AssetSerial.ToLower().Contains(serial.ToLower()));
            }

            // No hay registros
            if (query.ToList().Count() == 0)
            {
                ViewData["emptyMessage"] = "No se encontraron resultados";
            }
            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convierte por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasamos el query de Equipments por el modelo de paginación
            return View(await PaginatedList<Equipment>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public async Task<JsonResult> GetEquipmentById(int id)
        {
            var equipment = await logicEquipment.All()
                                .Include(e => e.Device).ThenInclude(x => x.Model).ThenInclude(x => x.Category)
                                .Include(e => e.EquipmentGroup)
                                .FirstOrDefaultAsync(m => m.Id == id);
            var customerInfo = await _context.CustomerInfos.Where(x => x.Id == GetCustomerInfoId())
                                    .FirstOrDefaultAsync();
            TimeSpan span = DateTime.UtcNow.AddHours(customerInfo.ZoneTime).Subtract(equipment.LastDataTx);
            double diffMinutes = span.TotalMinutes;
            return Json(new { equipment, diffMinutes });
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: Equipments/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var customerInfoId = GetCustomerInfoId();
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId == customerInfoId).OrderBy(x => x.Id);
            ViewData["EquipmentGroupId"] = new SelectList(equipmentGroups, "Id", "Title");
            ViewData["categories"] = _context.Categories.OrderBy(x => x.Id).ToList();
            return View();
        }

        /*
         * AJAX
         * Se encarga de retornar los models por una categoría seleccionada
         */
        public JsonResult GetModelsByCategoryId(int categoryId)
        {
            var models = from m in _context.Models
                         where m.CategoryId == categoryId
                         orderby m.Id
                         select m;
            return Json(models);
        }

        /*
         * AJAX
         * Se encarga de retornar los devices por un modelo seleccionado
         */
        public JsonResult GetDevicesByModelId(int modelId)
        {
            // Obtener el customer_info_id de los Claims del login
            int customerInfoId = GetCustomerInfoId();
            // Obtener los Devices que ya fueron asignados anteriormente
            var assignedDevices = _context.Equipments.Include(x => x.Device)
                                   .Select(x => x.Device);
            var devices = _context.Devices
                           .Where(x => x.CustomerInfoId == customerInfoId)
                           .Where(x => x.ModelId == modelId)
                           .OrderBy(x => x.Id);
            // Except para excluir los dispositivos que ya fueron asignados de la consulta
            var data = devices.Except(assignedDevices);
            return Json(data);
        }

        // POST: Equipments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EquipmentAlias,DeviceId,Description,EquipmentGroupId,NotifyInfo")] Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                await logicEquipment.Create(equipment);
                return RedirectToAction(nameof(Index));
            }
            var customerInfoId = GetCustomerInfoId();
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId ==customerInfoId).OrderBy(x => x.Id);
            ViewData["EquipmentGroupId"] = new SelectList(equipmentGroups, "Id", "Title");
            ViewData["categories"] = _context.Categories.OrderBy(x => x.Id).ToList();
            return View(equipment);
        }

        // GET: Equipments/Edit/5
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

            var equipment = await logicEquipment.Find(id);
            if (equipment == null)
            {
                return NotFound();
            }
            var customerInfoId = GetCustomerInfoId();
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId == customerInfoId).OrderBy(x => x.Id);
            ViewData["EquipmentGroupId"] = new SelectList(equipmentGroups, "Id", "Title", equipment.EquipmentGroupId);
            return View(equipment);
        }

        // POST: Equipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EquipmentAlias,Description,EquipmentGroupId,NotifyInfo")] Equipment equipment)
        {
            if (id != equipment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Modificar solamente los atributos que se envíaron y mantener los demás como están
                var modelEdit = await logicEquipment.Find(id);
                modelEdit.EquipmentAlias = equipment.EquipmentAlias;
                modelEdit.EquipmentGroupId = equipment.EquipmentGroupId;
                modelEdit.Description = equipment.Description;
                modelEdit.NotifyInfo = equipment.NotifyInfo;
                try
                {
                    await logicEquipment.Update(modelEdit);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipmentExists(equipment.Id))
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
            var customerInfoId = GetCustomerInfoId();
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id);
            ViewData["EquipmentGroupId"] = new SelectList(equipmentGroups, "Id", "Title", equipment.EquipmentGroupId);
            return View(equipment);
        }

        // GET: Equipments/Delete/5
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

            var equipment = await logicEquipment.All()
                .Include(e => e.Device).ThenInclude(x => x.Model).ThenInclude(x => x.Category)
                .Include(e => e.EquipmentGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // POST: Equipments/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await logicEquipment.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// Si es un usuario administrador entonces se le da acceso sin verificar sus permisos de acceso
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener nombre del controlador
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains(nameController);
        }
    }
}
