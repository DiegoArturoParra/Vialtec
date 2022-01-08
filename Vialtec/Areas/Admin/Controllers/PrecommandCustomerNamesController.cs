using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Vialtec.Models;
using System.Security.Claims;
using Logica;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PrecommandCustomerNamesController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LPrecommandCustomerName _logicPrecommandCustomerName;

        public PrecommandCustomerNamesController(VialtecContext context)
        {
            _context = context;
            _logicPrecommandCustomerName = new LPrecommandCustomerName(context);
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            if (!await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el distribuor_info_id de los Claims del login
            int distributorInfoId = GetDistributorInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas de filtros
            // ...

            // Consulta de registros
            var query = _logicPrecommandCustomerName.All();

            // Include Precommand->Model y CustomerInfo
            query = query.Include(x => x.Precommand).ThenInclude(x => x.Model).Include(x => x.CustomerInfo);

            // Agrupar la consulta por customerInfoId
            var groupQuery = query.GroupBy(x => x.CustomerInfo);

            // Si no hay registros
            if (groupQuery.ToList().Count() == 0)
            {
                ViewData["emptyMessage"] = "No se encontraron resultados";
            }
            // Calcular el número de páginas
            decimal result = decimal.Divide(groupQuery.ToList().Count(), pageSize);
            // Convierte por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasar el query por el modelo de paginación
            return View(await PaginatedList<IGrouping<CustomerInfo, PrecommandCustomerName>>.CreateAsync(groupQuery.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        private int GetDistributorInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
        }

        // GET: Admin/PrecommandCustomerNames/Create
        public IActionResult Create()
        {
            var customerInfos = _context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId());
            ViewData["CustomerInfoId"] = new SelectList(customerInfos, "Id", "Title");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View();
        }

        // POST: Admin/PrecommandCustomerNames/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PrecommandId,CustomerInfoId,CustomerTitle")] PrecommandCustomerName precommandCustomerName)
        {
            if (ModelState.IsValid)
            {
                await _logicPrecommandCustomerName.Create(precommandCustomerName);
                return RedirectToAction(nameof(Index));
            }
            return View(precommandCustomerName);
        }

        // GET: Admin/PrecommandCustomerNames/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precommandCustomerName = await _logicPrecommandCustomerName.All()
                                        .Include(x => x.Precommand).ThenInclude(x => x.Model)
                                        .FirstOrDefaultAsync(x => x.Id == id);
            if (precommandCustomerName == null)
            {
                return NotFound();
            }
            // Obtener valores actuales
            int categoryId = precommandCustomerName.Precommand.Model.CategoryId;
            int modelId = precommandCustomerName.Precommand.ModelId;
            int precommandId = precommandCustomerName.PrecommandId;

            // Consultar según los valores actuales
            var customerInfos = _context.CustomerInfos.Where(x => x.DistributorInfoId == GetDistributorInfoId());
            var categories = _context.Categories;
            var models = _context.Models.Where(x => x.CategoryId == categoryId);
            var precommands = _context.Precommands.Where(x => x.ModelId == modelId);

            // Crear los ViewData para la vista
            ViewData["CustomerInfoId"] = new SelectList(customerInfos, "Id", "Title", precommandCustomerName.CustomerInfoId);
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Title", categoryId);
            ViewData["ModelId"] = new SelectList(models, "Id", "Title", modelId);
            ViewData["PrecommandId"] = new SelectList(precommands, "Id", "Title", precommandId);

            return View(precommandCustomerName);
        }

        // POST: Admin/PrecommandCustomerNames/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PrecommandId,CustomerInfoId,CustomerTitle")] PrecommandCustomerName precommandCustomerName)
        {
            if (id != precommandCustomerName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicPrecommandCustomerName.Update(precommandCustomerName);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrecommandCustomerNameExists(precommandCustomerName.Id))
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
            return View(precommandCustomerName);
        }

        // GET: Admin/PrecommandCustomerNames/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precommandCustomerName = await _logicPrecommandCustomerName.All()
                .Include(p => p.CustomerInfo)
                .Include(p => p.Precommand)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (precommandCustomerName == null)
            {
                return NotFound();
            }

            return View(precommandCustomerName);
        }

        // POST: Admin/PrecommandCustomerNames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicPrecommandCustomerName.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool PrecommandCustomerNameExists(int id)
        {
            return _context.PrecommandCustomerNames.Any(e => e.Id == id);
        }

        /// <summary>
        /// Se encarga de determinar si la relación entre precomando y cliente existe o no todavía
        /// </summary>
        /// <param name="precommandId"></param>
        /// <param name="customerInfoId"></param>
        /// <returns></returns>
        public async Task<JsonResult> RelationshipExists(int precommandId, int customerInfoId)
        {
            var exists = await _logicPrecommandCustomerName.RelationshipExists(precommandId, customerInfoId);
            return Json(exists);
        }

        /// <summary>
        /// Se encarga de determinar si la relación entre precomando y clietne existe o no todavía, teniendo en cuenta la actualización
        /// </summary>
        /// <param name="precommandId"></param>
        /// <param name="customerInfoId"></param>
        /// <returns></returns>
        public async Task<JsonResult> RelationshipExistsUpdate(int id, int precommandId, int customerInfoId)
        {
            var exists = await _logicPrecommandCustomerName.RelationshipExistsUpdate(id, precommandId, customerInfoId);
            return Json(exists);
        }

        /// <summary>
        /// Método encargado de retornar los modelos filtrados por el categotyId
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Models</returns>
        public async Task<JsonResult> GetModelsByCategoryId(int categoryId)
        {
            var models = _context.Models.Where(x => x.CategoryId == categoryId);
            return Json(await models.ToListAsync());
        } 

        /// <summary>
        /// Se encargar de retornar los precommands filtraods por el modelId
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns>Precommands</returns>
        public async Task<JsonResult> GetPrecommandsByModelId(int modelId)
        {
            var precommands = _context.Precommands.Where(x => x.ModelId == modelId);
            return Json(await precommands.ToListAsync());
        }

        /// <summary>
        /// Se encarga de determinar si el distributorUser autenticado tiene acceso a al controlador/vistas
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AccessGranted()
        {
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            int distributorUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorUserId").Value);
            var customerUser = await _context.DistributorUsers
                                .Include(x => x.SecurityProfileDist)
                                .ThenInclude(x => x.ProfilePermissionsDists)
                                .ThenInclude(x => x.SinglePermissionDist)
                                .FirstOrDefaultAsync(x => x.Id == distributorUserId);
            var singlePermissionIdentifiers = customerUser.SecurityProfileDist.ProfilePermissionsDists
                                                .Select(x => x.SinglePermissionDist.Identifier).ToList();
            return singlePermissionIdentifiers.Contains(nameController);
        }
    }
}
