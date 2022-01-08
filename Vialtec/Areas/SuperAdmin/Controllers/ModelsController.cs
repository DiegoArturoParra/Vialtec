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
    public class ModelsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LModel _logicModel;

        public ModelsController(VialtecContext context)
        {
            _context = context;
            _logicModel = new LModel(context);
        }

        // GET: SuperAdmin/Models
        public async Task<IActionResult> Index(int? pageNumber, string nombre, int? categoryId)
        {
            // Número de registros por página
            int pageSize = 10;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;
            ViewData["categoryId"] = categoryId;
            // ViewData para el select de categorías
            ViewData["categories"] = _context.Categories.ToList();

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = _logicModel.All();

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Filtro categoría
            if (categoryId != null && categoryId != -1)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            // Include Category
            query = query.Include(x => x.Category).Include(x => x.EncodingType);

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
            return View(await PaginatedList<Model>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/Models/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            ViewData["EncodingTypeId"] = new SelectList(_context.EncodingTypes, "Id", "Title");
            return View();
        }

        // POST: SuperAdmin/Models/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,CategoryId,EncodingTypeId")] Model model)
        {
            if (ModelState.IsValid)
            {
                await _logicModel.Create(model);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", model.CategoryId);
            ViewData["EncodingTypeId"] = new SelectList(_context.EncodingTypes, "Id", "Title", model.EncodingTypeId);
            return View(model);
        }

        // GET: SuperAdmin/Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _logicModel.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", model.CategoryId);
            ViewData["EncodingTypeId"] = new SelectList(_context.EncodingTypes, "Id", "Title", model.EncodingTypeId);
            return View(model);
        }

        // POST: SuperAdmin/Models/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CategoryId,EncodingTypeId")] Model model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicModel.Update(model);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelExists(model.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", model.CategoryId);
            ViewData["EncodingTypeId"] = new SelectList(_context.EncodingTypes, "Id", "Title", model.EncodingTypeId);
            return View(model);
        }

        // GET: SuperAdmin/Models/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _logicModel.All()
                .Include(m => m.Category)
                .Include(m => m.EncodingType)
                .FirstOrDefaultAsync(m => m.Id == id);

            // ForeignKey constraints (devices | modelEvents)
            var devices = _context.Devices.Where(x => x.ModelId == id);
            var modelEvents = _context.ModelEvents.Where(x => x.ModelId == id);

            if (devices.Count() != 0 || modelEvents.Count() != 0)
            {
                ViewData["foreignKeyMessage"] = "El modelo no puede ser eliminado porque está siendo utilizado en otras tablas";
            }

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: SuperAdmin/Models/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicModel.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }
    }
}
