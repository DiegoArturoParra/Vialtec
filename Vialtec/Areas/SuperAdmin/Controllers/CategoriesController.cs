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
    public class CategoriesController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCategory _logicCategory;

        public CategoriesController(VialtecContext context)
        {
            _context = context;
            _logicCategory = new LCategory(context);
        }

        // GET: SuperAdmin/Categories
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            // Número de registros por página
            int pageSize = 10;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = _logicCategory.All();

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Cuando no hay registros
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
            return View(await PaginatedList<Category>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SuperAdmin/Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title")] Category category)
        {
            if (ModelState.IsValid)
            {
                await _logicCategory.Create(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: SuperAdmin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _logicCategory.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: SuperAdmin/Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicCategory.Update(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: SuperAdmin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _logicCategory.All()
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verificar restricción por llave foranea
            var models = _context.Models.Where(x => x.CategoryId == id);
            if (models.Count() != 0)
            {
                ViewData["foreignKeyMessage"] = "La categoría no puede ser eliminada porque está siendo utilizada por uno o más modelos";
            }

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: SuperAdmin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicCategory.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
