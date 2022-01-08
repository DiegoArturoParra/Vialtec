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
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class DistributorInfosController : Controller
    {
        private readonly VialtecContext _context;
        private LDistributorInfo _logicDistributorInfo;

        public DistributorInfosController(VialtecContext context)
        {
            _context = context;
            _logicDistributorInfo = new LDistributorInfo(context);
        }

        // GET: SuperAdmin/DistributorInfoes
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            // Número de registros por página
            int pageSize = 10;
            // Almacenará el total de páginas
            int totalPages = 0;

            // ViewDatas de filtro
            ViewData["nombre"] = nombre;

            // Consulta de registros DistributorInfo
            var query = _logicDistributorInfo.All();

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
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

            // Pasar el query de DistributorInfo por el modelo de paginación
            return View(await PaginatedList<DistributorInfo>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/DistributorInfoes/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmail(string email)
        {
            var distributorInfo = await _logicDistributorInfo.All()
                                        .Where(x => x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(distributorInfo == null); // is unique
        }

        // POST: SuperAdmin/DistributorInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Email,Active,Address,City,Country,ContactPerson")] DistributorInfo distributorInfo)
        {
            if (ModelState.IsValid)
            {
                await _logicDistributorInfo.Create(distributorInfo);
                return RedirectToAction(nameof(Index));
            }
            return View(distributorInfo);
        }

        // GET: SuperAdmin/DistributorInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distributorInfo = await _logicDistributorInfo.Find(id);
            if (distributorInfo == null)
            {
                return NotFound();
            }
            return View(distributorInfo);
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmailUpdate(int id, string email)
        {
            var distributorInfo = await _logicDistributorInfo.All()
                                        .Where(x => x.Id != id && x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(distributorInfo == null); // is unique
        }

        // POST: SuperAdmin/DistributorInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Email,Active,Address,City,Country,ContactPerson")] DistributorInfo distributorInfo)
        {
            if (id != distributorInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicDistributorInfo.Update(distributorInfo);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistributorInfoExists(distributorInfo.Id))
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
            return View(distributorInfo);
        }

        // GET: SuperAdmin/DistributorInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distributorInfo = await _logicDistributorInfo.All()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (distributorInfo == null)
            {
                return NotFound();
            }

            // ForeignKey validación
            var distributorUsers = _context.DistributorUsers.Where(x => x.DistributorInfoId == id).ToList();
            var customerInfos = _context.CustomerInfos.Where(x => x.DistributorInfoId == id).ToList();

            if (distributorUsers.Count() != 0 || customerInfos.Count() != 0)
            {
                ViewData["foreignKeyMsg"] = "El distribuidor no puede ser eliminado porque está siendo utilizado en otras tablas";
            }

            return View(distributorInfo);
        }

        // POST: SuperAdmin/DistributorInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicDistributorInfo.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool DistributorInfoExists(int id)
        {
            return _context.DistributorInfos.Any(e => e.Id == id);
        }
    }
}
