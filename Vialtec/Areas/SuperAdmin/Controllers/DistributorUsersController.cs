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
    public class DistributorUsersController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LDistributorUser _logicDistributorUser;

        public DistributorUsersController(VialtecContext context)
        {
            _context = context;
            _logicDistributorUser = new LDistributorUser(context);
        }

        // GET: SuperAdmin/DistributorUsers
        public async Task<IActionResult> Index(int? pageNumber, string email)
        {
            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas filtro
            ViewData["email"] = email;

            // Consulta de registros distributorUser
            var query = _logicDistributorUser.All();

            // Filtro email
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(x => x.Email.ToLower().Contains(email.ToLower()));
            }

            // Include CustomerInfo
            query = query.Include(e => e.DistributorInfo).Include(x => x.SecurityProfileDist);

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

            // Pasar el query de DistributorUser por el modelo de paginación
            return View(await PaginatedList<DistributorUser>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        /*
         * Ajax
         * Se encarga de retornar los singlePermissions del distributorUserId
         */
        [HttpGet]
        public async Task<JsonResult> GetSinglePermissionsByDistributorUser(int distributorUserId)
        {
            var distributorUser = await _context.DistributorUsers
                              .Include(x => x.SecurityProfileDist).FirstOrDefaultAsync(x => x.Id == distributorUserId);
            var singelPermissionsTitles = _context.ProfilePermissionDists
                                .Include(x => x.SinglePermissionDist)
                                .Where(x => x.SecurityProfileDistId == distributorUser.SecurityProfileDistId)
                                .Select(x => x.SinglePermissionDist.Title)
                                .ToList();
            return Json(new { securityProfileTitle = distributorUser.SecurityProfileDist.Title, singelPermissionsTitles });
        }

        // GET: SuperAdmin/DistributorUsers/Create
        public IActionResult Create()
        {
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title");
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmail(string email)
        {
            var distributorUser = await _logicDistributorUser.All()
                                        .Where(x => x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(distributorUser == null); // is unique
        }

        public JsonResult GetSecurityProfileDistByDistributorId(int distributorInfoId)
        {
            var results = _context.SecurityProfileDists.Where(x => x.DistributorInfoId == distributorInfoId);
            return Json(results);
        }

        // POST: SuperAdmin/DistributorUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,PassKey,SecurityProfileDistId,DistributorInfoId")] DistributorUser distributorUser)
        {
            if (ModelState.IsValid)
            {
                await _logicDistributorUser.Create(distributorUser);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos, "Id", "Title", distributorUser.DistributorInfoId);
            return View(distributorUser);
        }

        // GET: SuperAdmin/DistributorUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distributorUser = await _logicDistributorUser.Find(id);
            if (distributorUser == null)
            {
                return NotFound();
            }
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", distributorUser.DistributorInfoId);
            var securityProfiles = _context.SecurityProfileDists
                                    .Where(x => x.DistributorInfoId == distributorUser.DistributorInfoId).OrderBy(x => x.Id);
            ViewData["SecurityProfileDistId"] = new SelectList(securityProfiles, "Id", "Title", distributorUser.SecurityProfileDistId);
            return View(distributorUser);
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmailUpdate(int id, string email)
        {
            var distributorUser = await _logicDistributorUser.All()
                                        .Where(x => x.Id != id && x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(distributorUser == null); // is unique
        }

        // POST: SuperAdmin/DistributorUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PassKey,SecurityProfileDistId,DistributorInfoId")] DistributorUser distributorUser)
        {
            if (id != distributorUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicDistributorUser.Update(distributorUser);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistributorUserExists(distributorUser.Id))
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
            ViewData["DistributorInfoId"] = new SelectList(_context.DistributorInfos.OrderBy(x => x.Id), "Id", "Title", distributorUser.DistributorInfoId);
            return View(distributorUser);
        }

        // GET: SuperAdmin/DistributorUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distributorUser = await _logicDistributorUser.All()
                .Include(d => d.DistributorInfo).Include(x => x.SecurityProfileDist)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (distributorUser == null)
            {
                return NotFound();
            }

            return View(distributorUser);
        }

        // POST: SuperAdmin/DistributorUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicDistributorUser.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool DistributorUserExists(int id)
        {
            return _context.DistributorUsers.Any(e => e.Id == id);
        }
    }
}
