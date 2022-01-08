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
using Vialtec.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class EquipmentGroupsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LEquipmentGroup logicEquipmentGroup;

        public EquipmentGroupsController(VialtecContext context)
        {
            _context = context;
            logicEquipmentGroup = new LEquipmentGroup(context);
        }

        // GET: EquipmentGroups
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Obtener el customerInfoId de los Claims definidos en la autentificación
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = logicEquipmentGroup.All()
                                    .Where(x => x.CustomerInfoId == customerInfoId);

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Include CustomerInfo
            query = query.Include(e => e.CustomerInfo);

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
            return View(await PaginatedList<EquipmentGroup>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: EquipmentGroups/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["customerInfoId"] = GetCustomerInfoId();
            return View();
        }

        // POST: EquipmentGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerInfoId,Title,Description")] EquipmentGroup equipmentGroup)
        {
            if (ModelState.IsValid)
            {
                await logicEquipmentGroup.Create(equipmentGroup);
                return RedirectToAction(nameof(Index));
            }
            ViewData["customerInfoId"] = GetCustomerInfoId();
            return View(equipmentGroup);
        }

        // GET: EquipmentGroups/Edit/5
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

            var equipmentGroup = await logicEquipmentGroup.Find(id);
            if (equipmentGroup == null)
            {
                return NotFound();
            }
            return View(equipmentGroup);
        }

        // POST: EquipmentGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,Title,Description")] EquipmentGroup equipmentGroup)
        {
            if (id != equipmentGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await logicEquipmentGroup.Update(equipmentGroup);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipmentGroupExists(equipmentGroup.Id))
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
            return View(equipmentGroup);
        }

        // GET: EquipmentGroups/Delete/5
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

            var equipmentGroup = await logicEquipmentGroup.All()
                .Include(e => e.CustomerInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            /*
             *  EquipmentGroups foreignKey Equipments
             *  Buscar si algún equipment está relacionado con el equipmentGroup que se quiere eliminar 
             *  para impedirlo en la vista
             */
            ViewData["equipments"] = _context.Equipments
                                     .Where(x => x.EquipmentGroupId.Equals(id))
                                     .ToList();

            if (equipmentGroup == null)
            {
                return NotFound();
            }

            return View(equipmentGroup);
        }

        // POST: EquipmentGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await logicEquipmentGroup.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EquipmentGroupExists(int id)
        {
            return _context.EquipmentGroups.Any(e => e.Id == id);
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
