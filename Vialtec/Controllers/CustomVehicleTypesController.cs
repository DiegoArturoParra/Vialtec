using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Logica;
using System;
using System.Security.Claims;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    public class CustomVehicleTypesController : Controller
    {
        private readonly LCustomVehicleType _logicCustomVehicleType;
        private readonly LSpeedReportCustomer _logicSpeedReportCustomer;
        private readonly LVehicleType _logicVehicleType;

        public CustomVehicleTypesController(VialtecContext context)
        {
            _logicVehicleType = new LVehicleType(context);
            _logicCustomVehicleType = new LCustomVehicleType(context);
            _logicSpeedReportCustomer = new LSpeedReportCustomer(context);
        }

        // GET: CustomVehicleTypes
        public async Task<IActionResult> Index(int? pageNumber, int? speedReportCustomerId, int? vehicleTypeId, string customTitle)
        {
            // Número de registros por página
            int pageSize = 10;
            // Número de páginas que serán generadas
            int totalPages = 0;

            // ViewDatas
            ViewData["speedReportCustomerId"] = speedReportCustomerId;
            ViewData["vehicleTypeId"] = vehicleTypeId;
            ViewData["customTitle"] = customTitle;

            // asp-items (select)
            ViewData["speedReportCustomers"] = await _logicSpeedReportCustomer.All()
                                                .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                                .OrderBy(x => x.Id)
                                                .ToListAsync();
            ViewData["vehicleTypes"] = await _logicVehicleType.All()
                                            .OrderBy(x => x.Id)
                                            .ToListAsync();

            // Consulta original
            var query = _logicCustomVehicleType.All();

            // Includes necesarios
            query = query.Include(x => x.SpeedReportCustomer).Include(x => x.VehicleType);

            // Filtrar por el actual cliente
            query = query.Where(x => x.SpeedReportCustomer.CustomerInfoId == GetCustomerInfoId());

            // Aplicar demás filtros...
            if (speedReportCustomerId != null && speedReportCustomerId != -1)
            {
                query = query.Where(x => x.SpeedReportCustomerId == speedReportCustomerId);
            }

            if (vehicleTypeId != null && vehicleTypeId != -1)
            {
                query = query.Where(x => x.VehicleTypeId == vehicleTypeId);
            }

            if (!string.IsNullOrEmpty(customTitle))
            {
                query = query.Where(x => x.CustomTitle.ToLower().Contains(customTitle.ToLower()));
            }

            // Total pages
            totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize); // 8.3 => 9
            ViewData["totalPages"] = totalPages;

            return View(await PaginatedList<CustomVehicleType>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: CustomVehicleTypes/Create
        public IActionResult Create()
        {
            var speedReportsCustomer = _logicSpeedReportCustomer.All()
                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                        .OrderBy(x => x.Id);
            ViewData["SpeedReportCustomerId"] = new SelectList(speedReportsCustomer, "Id", "Title");
            ViewData["VehicleTypeId"] = new SelectList(_logicVehicleType.All(), "Id", "Title");
            return View();
        }

        // POST: CustomVehicleTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehicleTypeId,SpeedReportCustomerId,CustomTitle,Picture")] CustomVehicleType customVehicleType)
        {
            if (ModelState.IsValid)
            {
                await _logicCustomVehicleType.Create(customVehicleType);
                return RedirectToAction(nameof(Index));
            }
            var speedReportsCustomer = _logicSpeedReportCustomer.All()
                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                        .OrderBy(x => x.Id);
            ViewData["SpeedReportCustomerId"] = new SelectList(speedReportsCustomer, "Id", "Title", customVehicleType.SpeedReportCustomerId);
            ViewData["VehicleTypeId"] = new SelectList(_logicVehicleType.All(), "Id", "Title", customVehicleType.VehicleTypeId);
            return View(customVehicleType);
        }

        // GET: CustomVehicleTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customVehicleType = await _logicCustomVehicleType.Find(id);
            if (customVehicleType == null)
            {
                return NotFound();
            }
            var speedReportsCustomer = _logicSpeedReportCustomer.All()
                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                        .OrderBy(x => x.Id);
            ViewData["SpeedReportCustomerId"] = new SelectList(speedReportsCustomer, "Id", "Title", customVehicleType.SpeedReportCustomerId);
            ViewData["VehicleTypeId"] = new SelectList(_logicVehicleType.All(), "Id", "Title", customVehicleType.VehicleTypeId);
            return View(customVehicleType);
        }

        // POST: CustomVehicleTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleTypeId,SpeedReportCustomerId,CustomTitle,Picture")] CustomVehicleType customVehicleType)
        {
            if (id != customVehicleType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicCustomVehicleType.Update(customVehicleType);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_logicCustomVehicleType.Exists(customVehicleType.Id))
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
            var speedReportsCustomer = _logicSpeedReportCustomer.All()
                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                        .OrderBy(x => x.Id);
            ViewData["SpeedReportCustomerId"] = new SelectList(speedReportsCustomer, "Id", "Title", customVehicleType.SpeedReportCustomerId);
            ViewData["VehicleTypeId"] = new SelectList(_logicVehicleType.All(), "Id", "Title", customVehicleType.VehicleTypeId);
            return View(customVehicleType);
        }

        // GET: CustomVehicleTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customVehicleType = await _logicCustomVehicleType.All()
                .Include(c => c.SpeedReportCustomer).Include(x => x.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customVehicleType == null)
            {
                return NotFound();
            }

            return View(customVehicleType);
        }

        // POST: CustomVehicleTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicCustomVehicleType.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Obtener el Customer Info Id actual
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }
    }
}
