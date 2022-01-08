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
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DevicesController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LDevice _logicDevice;

        public DevicesController(VialtecContext context)
        {
            _context = context;
            _logicDevice = new LDevice(context);
        }

        // GET: Admin/Devices
        public async Task<IActionResult> Index(int? pageNumber, string serial, int? customerInfoId)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el distribuor_info_id de los Claims del login
            int distributorInfoId = GetDistributorInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas de filtros
            ViewData["serial"] = serial;
            ViewData["customerInfoId"] = customerInfoId;
            ViewData["customerInfos"] = _context.CustomerInfos.Where(x => x.DistributorInfoId == distributorInfoId).ToList();

            // consulta de registros Devices por customerInfoId
            // La primera consulta trae los Devices pero no trae los que tengan customerInfoId = null
            var query1 = _logicDevice.All().Include(x => x.CustomerInfo).Include(x => x.Model)
                         .Where(x => x.DistributorInfoId == distributorInfoId);
            // La segunda consulta trae los Devices con customerInfoId == null
            var query2 = _logicDevice.All()
                         .Where(x => x.DistributorInfoId == distributorInfoId && x.CustomerInfoId == null);
            // La unión de las dos consultas
            var query = query1.Union(query2);

            // Filtro serial
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(x => x.AssetSerial.Contains(serial));
            }

            // Filtro customerInfoId
            if (customerInfoId != null && customerInfoId != -1)
            {
                query = query.Where(x => x.CustomerInfoId == customerInfoId);
            }

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

            // Pasar el query Device por el modelo de paginación
            return View(await PaginatedList<Device>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetDistributorInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
        }

        // GET: Admin/Devices/Edit/5
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

            var device = await _logicDevice.Find(id);
            if (device == null)
            {
                return NotFound();
            }

            int distributorInfoId = GetDistributorInfoId();
            var customerInfos = _context.CustomerInfos.Where(x => x.DistributorInfoId == distributorInfoId).OrderBy(x => x.Id);
            ViewData["customerInfos"] = customerInfos.ToList();
            return View(device);
        }

        // POST: Admin/Devices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,ZoneTime")] Device device)
        {
            if (id != device.Id)
            {
                return NotFound();
            }
            var currentDevice = await _logicDevice.Find(id);
            try
            {
                currentDevice.CustomerInfoId = device.CustomerInfoId == -1 ? null : device.CustomerInfoId;
                currentDevice.ZoneTime = device.ZoneTime;
                await _logicDevice.Update(currentDevice);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(device.Id))
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

        private bool DeviceExists(int id)
        {
            return _context.Devices.Any(e => e.Id == id);
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
