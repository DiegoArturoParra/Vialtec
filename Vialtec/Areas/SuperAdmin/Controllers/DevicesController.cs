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
using Vialtec.Models;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class DevicesController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LDevice _logicDevice;

        public DevicesController(VialtecContext context)
        {
            _context = context;
            _logicDevice = new LDevice(context);
        }

        // GET: SuperAdmin/Devices
        public async Task<IActionResult> Index(int? pageNumber, string serial, int? distributorInfoId)
        {

            // Número de registros por página
            int pageSize = 50;
            // Almacenará el total de páginas
            int totalPages = 0;

            // ViewDatas de filtros
            ViewData["serial"] = serial;
            ViewData["distributorInfoId"] = distributorInfoId;
            ViewData["distributorInfos"] = _context.DistributorInfos.OrderBy(x => x.Id).ToList();

            // La primera consutla regresa lo Devices pero excluye los que tengan customerInfoId = null
            var query1 = _logicDevice.All().Include(x => x.DistributorInfo).Include(x => x.CustomerInfo).Include(x => x.Model);
            // La segunda consulta regresa los Devices donde customerInfoId == null
            var query2 = _logicDevice.All().Include(x => x.DistributorInfo).Include(x => x.Model)
                         .Where(x => x.CustomerInfoId == null);
            // La unión de las dos consultas para obtener todos los registros
            var query = query1.Union(query2);

            // Filtro serial
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(x => x.AssetSerial.Contains(serial));
            }

            // Filtro distributorInfoId
            if (distributorInfoId != null && distributorInfoId != -1)
            {
                query = query.Where(x => x.DistributorInfoId == distributorInfoId);
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

            // Pasar el query de Devices por el modelo de paginación
            return View(await PaginatedList<Device>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/Devices/Create
        public IActionResult Create()
        {
            //ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title");
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title");
            ViewData["ModelId"] = new SelectList(_context.Models.OrderBy(x => x.Id), "Id", "Title");
            return View();
        }

        /*
         * Ajax
         * Se encargar de verificar si el AssetSerial que se quiere ingresar existe o no
         */
        [HttpGet]
        public async Task<JsonResult> AssetSerialExists(string assetSerial)
        {
            var device = await _context.Devices
                                .Where(x => x.AssetSerial.ToLower() == assetSerial.ToLower())
                                .FirstOrDefaultAsync();
            return Json(device != null);
        }

        /*
         * Ajax
         * Se encargar de verificar si el NetworkIdentifier que se quiere ingresar existe o no
         */
        [HttpGet]
        public async Task<JsonResult> NetworkIdentifierExists(string networkIdentifier)
        {
            var device = await _context.Devices
                                .Where(x => x.NetworkIdentifier.ToLower() == networkIdentifier.ToLower())
                                .FirstOrDefaultAsync();
            return Json(device != null);
        }

        /*
         * Ajax
         * Se encargar de verificar si el AssetSerial que se quiere ingresar existe o no, teniendo en cuenta que es un update
         */
        [HttpGet]
        public async Task<JsonResult> AssetSerialExistsUpdate(int id, string assetSerial)
        {
            var device = await _context.Devices
                                .Where(x => x.Id != id && x.AssetSerial.ToLower() == assetSerial.ToLower())
                                .FirstOrDefaultAsync();
            return Json(device != null);
        }

        /*
         * Ajax
         * Se encargar de verificar si el NetworkIdentifier que se quiere ingresar existe o no, teniendo en cuenta que es un update
         */
        [HttpGet]
        public async Task<JsonResult> NetworkIdentifierExistsUpdate(int id, string networkIdentifier)
        {
            var device = await _context.Devices
                                .Where(x => x.Id != id && x.NetworkIdentifier.ToLower() == networkIdentifier.ToLower())
                                .FirstOrDefaultAsync();
            return Json(device != null);
        }

        // POST: SuperAdmin/Devices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AssetSerial,NetworkIdentifier,ModelId,DistributorInfoId,ZoneTime,BluetoothInfo,DevPass")] Device device)
        {
            if (ModelState.IsValid)
            {
                device.CreationDT = DateTime.Now;
                await _logicDevice.Create(device);
                return RedirectToAction(nameof(Index));
            }
            //ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", device.CustomerInfoId);
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", device.DistributorInfoId);
            ViewData["ModelId"] = new SelectList(_context.Models.OrderBy(x => x.Id), "Id", "Title", device.ModelId);
            return View(device);
        }

        // GET: SuperAdmin/Devices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _logicDevice.Find(id);
            if (device == null)
            {
                return NotFound();
            }
            //ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", device.CustomerInfoId);
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", device.DistributorInfoId);
            ViewData["ModelId"] = new SelectList(_context.Models.OrderBy(x => x.Id), "Id", "Title", device.ModelId);
            return View(device);
        }

        // POST: SuperAdmin/Devices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetSerial,NetworkIdentifier,ModelId,CreationDT,CustomerInfoId,DistributorInfoId,ZoneTime,BluetoothInfo,DevPass")] Device device)
        {
            if (id != device.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicDevice.Update(device);
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
            //ViewData["CustomerInfoId"] = new SelectList(_context.CustomerInfos, "Id", "Title", device.CustomerInfoId);
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", device.DistributorInfoId);
            ViewData["ModelId"] = new SelectList(_context.Models.OrderBy(x => x.Id), "Id", "Title", device.ModelId);
            return View(device);
        }

        // GET: SuperAdmin/Devices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _logicDevice.All()
                .Include(d => d.CustomerInfo)
                .Include(d => d.DistributorInfo)
                .Include(d => d.Model)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Foreing Keys 
            var equipments = _context.Equipments.Where(x => x.DeviceId == id);
            if (equipments.Count() != 0)
            {
                ViewData["foreignKeyMessage"] = "El dispositivo está siendo utilizado en la tabla { equipments }.";
            }

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        // POST: SuperAdmin/Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicDevice.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool DeviceExists(int id)
        {
            return _context.Devices.Any(e => e.Id == id);
        }
    }
}
