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
    public class ModelEventsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LModelEvent _logicModelEvent;

        public ModelEventsController(VialtecContext context)
        {
            _context = context;
            _logicModelEvent = new LModelEvent(context);
        }

        // GET: SuperAdmin/ModelEvents
        public async Task<IActionResult> Index(int? pageNumber, int? modelId, int? eventId)
        {
            // Número de registros por página
            int pageSize = 20;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["modelId"] = modelId;
            ViewData["eventId"] = eventId;
            // ViewDatas para los select html
            ViewData["models"] = _context.Models.ToList();
            ViewData["events"] = _context.Events.ToList();

            // Consulta de registros EquipmentGroup por customerInfoId
            //ViewData["nombre"] = nombre;
            var query = _logicModelEvent.All();

            // Filtro Modelo
            if (modelId != null && modelId != -1)
            {
                query = query.Where(x => x.ModelId == modelId);
            }

            // Filtro Evento
            if (eventId != null && eventId != -1)
            {
                query = query.Where(x => x.EventId == eventId);
            }

            // Includes para Model y Event
            query = query.Include(x => x.Model).Include(x => x.Event);

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
            return View(await PaginatedList<ModelEvent>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/ModelEvents/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title");
            return View();
        }

        [HttpGet]
        public JsonResult VerifyRelationship(int modelId, int eventId)
        {
            var exists = _logicModelEvent.VerifyRelationshipExists(modelId, eventId);
            return Json(exists);
        }

        [HttpGet]
        public JsonResult VerifyRelationshipUpdate(int id, int modelId, int eventId)
        {
            var exists = _logicModelEvent.VerifyRelationshipExistsUpdate(id, modelId, eventId);
            return Json(exists);
        }

        // POST: SuperAdmin/ModelEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EventId,ModelId")] ModelEvent modelEvent)
        {
            if (ModelState.IsValid)
            {
                await _logicModelEvent.Create(modelEvent);
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", modelEvent.EventId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", modelEvent.ModelId);
            return View(modelEvent);
        }

        // GET: SuperAdmin/ModelEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelEvent = await _logicModelEvent.Find(id);
            if (modelEvent == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", modelEvent.EventId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", modelEvent.ModelId);
            return View(modelEvent);
        }

        // POST: SuperAdmin/ModelEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventId,ModelId")] ModelEvent modelEvent)
        {
            if (id != modelEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicModelEvent.Update(modelEvent);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelEventExists(modelEvent.Id))
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
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", modelEvent.EventId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Title", modelEvent.ModelId);
            return View(modelEvent);
        }

        // GET: SuperAdmin/ModelEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modelEvent = await _logicModelEvent.All()
                .Include(m => m.Event)
                .Include(m => m.Model)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verificar llave foranea con la tabla CustomerModelEvent
            var customerModelEvents = _context.CustomerModelEvents
                                    .Where(x => x.ModelEventId == id);
            if (customerModelEvents.Count() != 0)
            {
                ViewData["foreignKeyMessage"] = "El registro no puede ser eliminado " +
                                                "porque está siendo utilizado en otras tablas";
            }

            if (modelEvent == null)
            {
                return NotFound();
            }

            return View(modelEvent);
        }

        // POST: SuperAdmin/ModelEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicModelEvent.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ModelEventExists(int id)
        {
            return _context.ModelEvents.Any(e => e.Id == id);
        }
    }
}
