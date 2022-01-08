using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Logica;
using Vialtec.Models;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerInfosController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCustomerInfo _logicCustomerInfo;

        public CustomerInfosController(VialtecContext context)
        {
            _context = context;
            _logicCustomerInfo = new LCustomerInfo(context);
        }

        // GET: Admin/CustomerInfoes
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el distributor_info_id de los Claims del login
            int distributorInfoId = GetDistributorInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;

            // Consulta de registros CustomerInfo por DistributorInfoId
            var query = _logicCustomerInfo.All()
                                    .Where(x => x.DistributorInfoId == distributorInfoId);

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Include DistributorInfo
            query = query.Include(x => x.DistributorInfo);

            // Si no hay registros
            if (query.ToList().Count() == 0)
            {
                ViewData["emptyMessage"] = "No se encontraron resultados";
            }
            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convierte por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasamos el query de CustomerInfo por el modelo de paginación
            return View(await PaginatedList<CustomerInfo>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetDistributorInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
        }

        // GET: Admin/CustomerInfoes/Create
        public async Task<IActionResult> Create()
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el distributor_info_id de los Claims del login
            int distributorInfoId = GetDistributorInfoId();
            ViewData["DistributorInfoId"] = distributorInfoId;
            return View();
        }

        // POST: Admin/CustomerInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> Create([Bind("Id,Title,DistributorInfoId,LogoBase64,ZoneTime")] CustomerInfo customerInfo)
        {
            try
            {
                // Crear el CustomerInfo
                customerInfo.LogoBase64 = customerInfo.LogoBase64.Split(',')[1];
                await _logicCustomerInfo.Create(customerInfo);
                return Json(true);
            } catch(Exception)
            {
                return Json(false);
            }
        }

        // GET: Admin/CustomerInfoes/Edit/5
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

            var customerInfo = await _logicCustomerInfo.Find(id);
            if (customerInfo == null)
            {
                return NotFound();
            }
            return View(customerInfo);
        }

        // POST: Admin/CustomerInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,DistributorInfoId,LogoBase64,ZoneTime")] CustomerInfo customerInfo)
        {
            if (id != customerInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customerInfo.LogoBase64 = customerInfo.LogoBase64.Split(',')[1];
                    await _logicCustomerInfo.Update(customerInfo);
                    return Json(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerInfoExists(customerInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(customerInfo);
        }

        // GET: Admin/CustomerInfoes/Delete/5
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

            var customerInfo = await _logicCustomerInfo.All().Include(x => x.DistributorInfo)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verificar las siguientes tablas para las llaves foraneas
            var customerUsers = _context.CustomerUsers.Where(x => x.CustomerInfoId.Equals(id)).ToList();
            var devices = _context.Devices.Where(x => x.CustomerInfoId.Equals(id)).ToList();
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId.Equals(id)).ToList();
            var securityProfiles = _context.SecurityProfiles.Where(x => x.CustomerInfoId.Equals(id)).ToList();

            if (customerUsers.Count() != 0 || devices.Count() != 0 || equipmentGroups.Count() != 0 || securityProfiles.Count() != 0)
            {
                ViewData["foreignErrorMsg"] = "El cliente no se puede eliminar porque está siendo utilizado por otras tablas";
            }

            if (customerInfo == null)
            {
                return NotFound();
            }

            return View(customerInfo);
        }

        // POST: Admin/CustomerInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Eliminar securityProfiles
            foreach(var secProfile in _context.SecurityProfiles.Where(x => x.CustomerInfoId.Equals(id)))
            {
                _context.SecurityProfiles.Remove(secProfile);
            }
            await _logicCustomerInfo.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerInfoExists(int id)
        {
            return _context.CustomerInfos.Any(e => e.Id == id);
        }

        /*
        * Se encarga de determinar si el distributorUser autenticado tiene acceso a al controlador/vistas
        */
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
