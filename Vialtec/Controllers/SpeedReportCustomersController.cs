using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Logica;
using System.Security.Claims;
using Vialtec.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class SpeedReportCustomersController : Controller
    {
        private readonly LSpeedReportCustomer _logicSpeedReportCustomer;
        private readonly LCustomerInfo _logicCustomerInfo;
        private readonly LCustomVehicleType _logicCustomVehicleType;

        public SpeedReportCustomersController(VialtecContext context)
        {
            _logicCustomerInfo = new LCustomerInfo(context);
            _logicSpeedReportCustomer = new LSpeedReportCustomer(context);
            _logicCustomVehicleType = new LCustomVehicleType(context);
        }

        // GET: SpeedReportCustomers
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            // Número de registros por página
            int pageSize = 50;
            // Número de páginas que serán generadas
            int totalPages = 0;

            // ViewData
            ViewData["nombre"] = nombre;

            // Consulta
            var query = _logicSpeedReportCustomer.All();

            // Filtrar por el cliente actual
            query = query.Where(x => x.CustomerInfoId == GetCustomerInfoId());

            // Aplicar filtros
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Include
            query = query.Include(s => s.CustomerInfo);

            // Total pages
            totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize); // 8.3 => 9
            ViewData["totalPages"] = totalPages;

            return View(await PaginatedList<SpeedReportCustomer>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SpeedReportCustomers/Create
        public IActionResult Create()
        {
            ViewData["CustomerInfoId"] = GetCustomerInfoId();
            return View();
        }

        // POST: SpeedReportCustomers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,CustomerInfoId")] SpeedReportCustomer speedReportCustomer)
        {
            if (ModelState.IsValid)
            {
                await _logicSpeedReportCustomer.Create(speedReportCustomer);
                return RedirectToAction(nameof(Index));
            }
            return View(speedReportCustomer);
        }

        // GET: SpeedReportCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["CustomerInfoId"] = GetCustomerInfoId();
            var speedReportCustomer = await _logicSpeedReportCustomer.Find(id);
            if (speedReportCustomer == null)
            {
                return NotFound();
            }
            return View(speedReportCustomer);
        }

        // POST: SpeedReportCustomers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CustomerInfoId")] SpeedReportCustomer speedReportCustomer)
        {
            if (id != speedReportCustomer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicSpeedReportCustomer.Update(speedReportCustomer);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_logicSpeedReportCustomer.Exists(speedReportCustomer.Id))
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
            return View(speedReportCustomer);
        }

        // GET: SpeedReportCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var speedReportCustomer = await _logicSpeedReportCustomer.All()
                .Include(s => s.CustomerInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (speedReportCustomer == null)
            {
                return NotFound();
            }

            // ForeignKey verify
            var customVehicleType = await _logicCustomVehicleType.All()
                                            .FirstOrDefaultAsync(x => x.SpeedReportCustomerId == id);

            if (customVehicleType != null)
            {
                ViewData["foreignKeyError"] = "El reporte está siendo utilizado en la tabla de categorías.";
            }

            return View(speedReportCustomer);
        }

        // POST: SpeedReportCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicSpeedReportCustomer.Delete(id);
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
