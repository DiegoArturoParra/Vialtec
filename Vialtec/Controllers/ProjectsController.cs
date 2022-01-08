using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
    public class ProjectsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LProject _logicProject;

        public ProjectsController(VialtecContext context)
        {
            _context = context;
            _logicProject = new LProject(context);
        }

        // GET: Projects
        public async Task<IActionResult> Index(int? pageNumber, string nombre)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el customer_info_id de los Claims del login
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 50;
            // Almacenará el total de las páginas
            int totalPages = 0;

            // ViewDatas de filtros
            ViewData["nombre"] = nombre;

            // Consulta de registros Project por customerInfoId
            var query = _logicProject.All()
                                    .Where(x => x.CustomerInfoId == customerInfoId);

            // Filtro nombre
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Title.ToLower().Contains(nombre.ToLower()));
            }

            // Include CustomerInfo
            query = query.Include(e => e.CustomerInfo).Include(x => x.SyncSource);

            totalPages = (int)Math.Ceiling(query.Count() / (double)pageSize); // 8.3 => 9 
            ViewData["totalPages"] = totalPages;

            // Pasamos el query de Projects por el modelo de paginación
            return View(await PaginatedList<Project>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["customerInfoId"] = GetCustomerInfoId();
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerInfoId,Title,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                // Agregando atributos al model Project
                project.CreatedDate = DateTime.Now;
                project.UserAlias = User.Identity.Name;
                project.SyncSourceId = 2; // Web

                await _logicProject.Create(project);
                return RedirectToAction(nameof(Index));
            }
            ViewData["customerInfoId"] = GetCustomerInfoId();
            return View(project);
        }

        // GET: Projects/Edit/5
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

            var project = await _logicProject.Find(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerInfoId,Title,Description,CreatedDate")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // agregar atributos adicionales
                    project.SyncSourceId = 2; // Web
                    project.UserAlias = User.Identity.Name;

                    await _logicProject.Update(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            return View(project);
        }

        // GET: Projects/Delete/5
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

            var project = await _logicProject.All()
                .Include(p => p.CustomerInfo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            // Consultar los subprojects antes de eliminar el project
            var subprojects = _context.Subprojects.Where(x => x.ProjectId == id).ToList();
            ViewData["subprojects"] = subprojects;

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicProject.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
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
