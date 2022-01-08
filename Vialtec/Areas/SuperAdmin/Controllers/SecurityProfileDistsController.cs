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
    public class SecurityProfileDistsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LSecurityProfileDist _logicSecurityProfileDist;

        public SecurityProfileDistsController(VialtecContext context)
        {
            _context = context;
            _logicSecurityProfileDist = new LSecurityProfileDist(context);
        }

        // GET: SuperAdmin/SecurityProfileDists
        public async Task<IActionResult> Index(int? pageNumber, string nombre, int? distributorInfoId)
        {
            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas filtro
            ViewData["nombre"] = nombre;
            ViewData["distributorInfoId"] = distributorInfoId;
            ViewData["distributorInfos"] = _context.DistributorInfos.OrderBy(x => x.Id).ToList();

            // Consulta de registros SecurityProfile (distribuidores)
            var query = _logicSecurityProfileDist.All();

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }
            // Filtro distributorInfoId
            if (distributorInfoId != null && distributorInfoId != -1)
            {
                query = query.Where(x => x.DistributorInfoId == distributorInfoId);
            }

            // Include DistributorInfo
            query = query.Include(e => e.DistributorInfo)
                         .Include(x => x.ProfilePermissionsDists)
                         .ThenInclude(x => x.SinglePermissionDist);

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

            // Pasar el query de SecurityProfile (distribuidores) por el modelo de paginación
            return View(await PaginatedList<SecurityProfileDist>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: SuperAdmin/SecurityProfileDists/Create
        public IActionResult Create()
        {
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title");
            return View();
        }

        // POST: SuperAdmin/SecurityProfileDists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DistributorInfoId,Title")] SecurityProfileDist securityProfileDist)
        {
            if (ModelState.IsValid)
            {
                await _logicSecurityProfileDist.Create(securityProfileDist);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", securityProfileDist.DistributorInfoId);
            return View(securityProfileDist);
        }

        // GET: SuperAdmin/SecurityProfileDists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var securityProfileDist = await _logicSecurityProfileDist.Find(id);
            if (securityProfileDist == null)
            {
                return NotFound();
            }
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", securityProfileDist.DistributorInfoId);
            return View(securityProfileDist);
        }

        // POST: SuperAdmin/SecurityProfileDists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DistributorInfoId,Title")] SecurityProfileDist securityProfileDist)
        {
            if (id != securityProfileDist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicSecurityProfileDist.Update(securityProfileDist);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SecurityProfileDistExists(securityProfileDist.Id))
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
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", securityProfileDist.DistributorInfoId);
            return View(securityProfileDist);
        }

        // GET: SuperAdmin/SecurityProfileDists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var securityProfileDist = await _logicSecurityProfileDist.All()
                .Include(s => s.DistributorInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (securityProfileDist == null)
            {
                return NotFound();
            }

            // Comprobar que el perfil de seguridad NO está siendo utilizado por un distributorUser
            var distributorUsers = _context.DistributorUsers.Where(x => x.SecurityProfileDistId == id).ToList();
            if (distributorUsers.Count() != 0)
            {
                ViewData["foreignMsg"] = "El perfil no puede ser eliminado porque está siendo utilizado";
            }

            return View(securityProfileDist);
        }

        // POST: SuperAdmin/SecurityProfileDists/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Eliminar primero los registros en ProfilePermissions
            var profilePermissionsDists = _context.ProfilePermissionDists.Where(x => x.SecurityProfileDistId == id);
            _context.ProfilePermissionDists.RemoveRange(profilePermissionsDists);
            await _context.SaveChangesAsync();
            // Eliminar el Perfil de Seguridad
            await _logicSecurityProfileDist.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SecurityProfileDistExists(int id)
        {
            return _context.SecurityProfileDists.Any(e => e.Id == id);
        }
    }
}
