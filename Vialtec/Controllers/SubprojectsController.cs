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
using System.Security.Claims;
using Vialtec.Models;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class SubprojectsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LSubproject _logicSubproject;

        public SubprojectsController(VialtecContext context)
        {
            _context = context;
            _logicSubproject = new LSubproject(context);
        }

        // GET: Subprojects
        public async Task<IActionResult> Index(int? pageNumber, int? projectId, string nombre)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el customerInfoId de los Claims del login
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 50;
            // Almacenará el total de páginas
            int totalPages = 0;

            // ViewBag para el select de projects para el filtro
            ViewBag.projects = _context.Projects.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id).ToList();

            // ViewDatas filtro
            ViewData["nombre"] = nombre;
            ViewData["projectId"] = projectId;

            // Consulta de Subprojects por customerInfoId
            var query = _logicSubproject.All().Include(x => x.Project)
                                        .Where(x => x.Project.CustomerInfoId == customerInfoId);

            // Filtro project
            if (projectId != null && projectId != -1)
            {
                query = query.Where(x => x.ProjectId.Equals(projectId));
            }

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize); // 8.3 => 9 
            ViewData["totalPages"] = totalPages;

            // Pasar el query de Subprojects por el modelo de paginación
            return View(await PaginatedList<Subproject>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: Subprojects/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            int customerInfoId = GetCustomerInfoId();
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id), "Id", "Title");
            return View();
        }

        // POST: Subprojects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,Title,Description")] Subproject subproject)
        {
            var customerInfo = await _context.CustomerInfos.Where(x => x.Id == GetCustomerInfoId())
                                    .FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                // Agregando atributos al model Subproject 
                // DateTime teniendo en cuenta el Utc y ZoneTime del cliente
                subproject.CreatedDate = DateTime.UtcNow.AddHours(customerInfo.ZoneTime);
                subproject.UserAlias = User.Identity.Name;
                subproject.SyncSourceId = 2; // Web

                await _logicSubproject.Create(subproject);
                return RedirectToAction(nameof(Index));
            }
            int customerInfoId = GetCustomerInfoId();
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id), "Id", "Title", subproject.ProjectId);
            return View(subproject);
        }

        // GET: Subprojects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var subproject = await _logicSubproject.Find(id);
            if (subproject == null)
            {
                return NotFound();
            }
            int customerInfoId = GetCustomerInfoId();
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id), "Id", "Title", subproject.ProjectId);
            return View(subproject);
        }

        // POST: Subprojects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,CreatedDate")] Subproject subproject)
        {
            if (id != subproject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // agregar atributos adicionales
                    subproject.SyncSourceId = 2; // Web
                    subproject.UserAlias = User.Identity.Name;

                    await _logicSubproject.Update(subproject);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubprojectExists(subproject.Id))
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
            int customerInfoId = GetCustomerInfoId();
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.CustomerInfoId.Equals(customerInfoId)).OrderBy(x => x.Id), "Id", "Title", subproject.ProjectId);
            return View(subproject);
        }

        // GET: Subprojects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (id == null)
            {
                return NotFound();
            }

            var subproject = await _logicSubproject.All()
                .Include(s => s.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subproject == null)
            {
                return NotFound();
            }

            return View(subproject);
        }

        // POST: Subprojects/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken] ACTIVAR SI ES NECESARIO
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicSubproject.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool SubprojectExists(int id)
        {
            return _context.Subprojects.Any(e => e.Id == id);
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// Si es un usuario administrador entonces se le da acceso sin verificar sus permisos de acceso
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener nombre del controlador
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains(nameController);
        }
    }
}
