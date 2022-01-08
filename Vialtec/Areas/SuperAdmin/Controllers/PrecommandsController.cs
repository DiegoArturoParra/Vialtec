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
using Newtonsoft.Json;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class PrecommandsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LPrecommand _logicPrecommand;

        public PrecommandsController(VialtecContext context)
        {
            _context = context;
            _logicPrecommand = new LPrecommand(context);
        }

        // GET: SuperAdmin/Precommands
        public async Task<IActionResult> Index(int? pageNumber, int? modelId)
        {
            //if (!await AccessGranted())
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}

            // Número de registros por página
            int pageSize = 10;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["modelId"] = modelId;
            // ViewData para los select
            ViewData["models"] = _context.Models.ToList();

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = _logicPrecommand.All();

            // Filtro Model Id
            if (modelId != null && modelId != -1)
            {
                query = query.Where(x => x.ModelId == modelId);
            }

            // Include Model
            query = query.Include(x => x.Model);

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
            return View(await PaginatedList<Precommand>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/Precommands/Create
        public IActionResult Create()
        {
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title");
            return View();
        }

        // POST: SuperAdmin/Precommands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,CommandData,ExpectedAck,ModelId")] Precommand precommand)
        {
            precommand.ExpectedAck = JsonConvert.SerializeObject(
                                        new ExpectedAck{ acks = precommand.ExpectedAck.Split('#') }
                                    );
            if (ModelState.IsValid)
            {
                await _logicPrecommand.Create(precommand);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", precommand.ModelId);
            return View(precommand);
        }

        // GET: SuperAdmin/Precommands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precommand = await _logicPrecommand.Find(id);
            if (precommand == null)
            {
                return NotFound();
            }
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", precommand.ModelId);
            return View(precommand);
        }

        // POST: SuperAdmin/Precommands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,CommandData,ExpectedAck,ModelId")] Precommand precommand)
        {
            if (id != precommand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                precommand.ExpectedAck = JsonConvert.SerializeObject(
                                            new ExpectedAck { acks = precommand.ExpectedAck.Split('#') }
                                         );
                try
                {
                    await _logicPrecommand.Update(precommand);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrecommandExists(precommand.Id))
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
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", precommand.ModelId);
            return View(precommand);
        }

        // GET: SuperAdmin/Precommands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precommand = await _logicPrecommand.All()
                .Include(p => p.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (precommand == null)
            {
                return NotFound();
            }

            return View(precommand);
        }

        // POST: SuperAdmin/Precommands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicPrecommand.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool PrecommandExists(int id)
        {
            return _context.Precommands.Any(e => e.Id == id);
        }
    }
}
