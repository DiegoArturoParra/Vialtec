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
using Microsoft.AspNetCore.Authorization;
using Vialtec.Models;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class EventsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LEvent _logicEvent;

        public EventsController(VialtecContext context)
        {
            _context = context;
            _logicEvent = new LEvent(context);
        }

        // GET: SuperAdmin/Events
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            // Número de registros por página
            int pageSize = 20;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["nombre"] = nombre;

            // Consulta de registros events
            var query = _logicEvent.All();

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

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
            return View(await PaginatedList<Event>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SuperAdmin/Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KeyStr,Title")] Event @event)
        {
            if (ModelState.IsValid)
            {
                await _logicEvent.Create(@event);
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: SuperAdmin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _logicEvent.Find(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: SuperAdmin/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KeyStr,Title")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicEvent.Update(@event);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            return View(@event);
        }

        // GET: SuperAdmin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _logicEvent.All()
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verificar restricción de llave foranea

            // Reviscar ModelEvents
            var modelEvents = _context.ModelEvents.Where(x => x.EventId == id).ToList();
            /*
             * Revisar transmissions pero con saber que un solo registro lo tiene es suficiente
             * para evitar listar todo y que la consulta se demore
             */
            var transmissions = _context.TransmissionInfos.Where(x => x.EventId == id).FirstOrDefault();

            if (modelEvents.Count() != 0 || transmissions != null)
            {
                ViewData["foreignKeyMessage"] = "El evento no se puede eliminar porque está siendo utilizado en otras tablas";
            }

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: SuperAdmin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicEvent.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
