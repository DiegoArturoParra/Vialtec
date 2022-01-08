using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using Microsoft.AspNetCore.Authorization;
using Logica;
using System.Security.Claims;
using Vialtec.Models;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,CustomerAdmin")]
    public class CustomerUsersController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCustomerUser _logicCustomerUser;

        public CustomerUsersController(VialtecContext context)
        {
            _context = context;
            _logicCustomerUser = new LCustomerUser(context);
        }

        // GET: Admin/CustomerUsers
        public async Task<IActionResult> Index(int? pageNumber, string email, int? customerInfoId)
        {
            // Si NO es un customerAdmin (es decir es un Admin), y no tiene permisos para esta sección
            if (!User.IsInRole("CustomerAdmin") && !await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el distributor_info_id de los Claims del login
            int distributorInfoId = await GetDistributorInfoId();

            // Número de registros por página
            int pageSize = 10;
            // Almacenará el número de páginas
            int totalPages = 0;

            // ViewDatas para los select
            ViewData["customerInfos"] = _context.CustomerInfos
                                                .Where(x => x.DistributorInfoId == distributorInfoId)
                                                .ToList();
            // ViewDatas filtros
            ViewData["email"] = email;
            ViewData["customerInfoId"] = customerInfoId;

            // Consulta de CustomerUser por DistributorInfoId
            var query = _logicCustomerUser.All().Include(c => c.CustomerInfo).Include(c => c.SecurityProfile)
                                                .Where(x => x.CustomerInfo.DistributorInfoId == distributorInfoId);

            // Filtrar si el que solicita la información es un usuario administrador
            if (User.IsInRole("CustomerAdmin"))
            {
                int customerInfoIdClaim = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
                query = query.Where(x => x.CustomerInfoId == customerInfoIdClaim);
                int customerUserIdClaim = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
                query = query.Except(query.Where(x => x.Id == customerUserIdClaim));
            }

            // Filtro nombre
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(x => x.Email.ToLower().Contains(email.ToLower()));
            }

            // Filtro cliente
            if (customerInfoId != null && customerInfoId != -1)
            {
                query = query.Where(x => x.CustomerInfoId == customerInfoId);
            }

            // Ordenar consulta por cliente
            query = query.OrderBy(x => x.CustomerInfoId);

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

            // Pasar el query de CustomerUser por el modelo de paginación
            return View(await PaginatedList<CustomerUser>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/CustomerUsers/Create
        public async Task<IActionResult> Create()
        {
            // Si NO es un customerAdmin (es decir es un Admin), y no tiene permisos para esta sección
            if (!User.IsInRole("CustomerAdmin") && !await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Dependiendo de si el que entra en la vista es un distribuidor o un usuario administrador
            if (User.IsInRole("Admin")) // Distribuidor
            {
                int distributorInfoId = await GetDistributorInfoId();
                ViewData["CustomerInfoId"] = new SelectList(GetCustomerInfosByDist(distributorInfoId), "Id", "Title");
            }
            else if (User.IsInRole("CustomerAdmin"))
            {
                int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity)
                                            .FindFirst("customerInfoId").Value);
                ViewData["CustomerInfoId"] = new SelectList(
                    _context.CustomerInfos.Where(x => x.Id == customerInfoId), "Id", "Title"
                );
            }
            return View();
        }

        // POST: Admin/CustomerUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,PassKey,CustomerInfoId")] CustomerUser customerUser)
        {
            // Complementar información del customer user
            customerUser.SecurityProfileId = 1;
            customerUser.IsRoot = User.IsInRole("Admin"); // true or false, si es false es porque es un usuario administrador

            if (ModelState.IsValid)
            {
                await _logicCustomerUser.Create(customerUser);
                return RedirectToAction(nameof(Index));
            }
            return View(customerUser);
        }

        // GET: Admin/CustomerUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Si NO es un customerAdmin (es decir es un Admin), y no tiene permisos para esta sección
            if (!User.IsInRole("CustomerAdmin") && !await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var customerUser = await _logicCustomerUser.All()
                               .Include(x => x.CustomerInfo).FirstOrDefaultAsync(x => x.Id == id);
            if (customerUser == null)
            {
                return NotFound();
            }

            // Dependiendo de si el que entra en la vista es un distribuidor o un usuario administrador
            if (User.IsInRole("Admin"))
            {
                int distributorInfoId = await GetDistributorInfoId();
                ViewData["CustomerInfoId"] = new SelectList(
                    GetCustomerInfosByDist(distributorInfoId), 
                    "Id", "Title",
                    customerUser.CustomerInfoId
                );
            }
            else if (User.IsInRole("CustomerAdmin"))
            {
                int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity)
                                            .FindFirst("customerInfoId").Value);
                ViewData["CustomerInfoId"] = new SelectList(
                    _context.CustomerInfos.Where(x => x.Id == customerInfoId),
                    "Id", "Title",
                    customerUser.CustomerInfoId
                );
            }
            return View(customerUser);
        }

        // POST: Admin/CustomerUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PassKey,CustomerInfoId")] CustomerUser customerUser)
        {
            if (id != customerUser.Id)
            {
                return NotFound();
            }

            // Complementar información del customer user
            customerUser.SecurityProfileId = 1;
            customerUser.IsRoot = User.IsInRole("Admin"); // true or false, si es false es porque es un usuario administrador

            if (ModelState.IsValid)
            {
                try
                {
                    await _logicCustomerUser.Update(customerUser);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerUserExists(customerUser.Id))
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
            return View(customerUser);
        }

        // GET: Admin/CustomerUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // El customerAdmin no tiene permiso para eliminar usuarios, 
            // después se verifica los permisos del distribuidor
            if (!User.IsInRole("CustomerAdmin") && !await AccessGranted())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var customerUser = await _logicCustomerUser.All()
                .Include(c => c.CustomerInfo)
                .Include(c => c.SecurityProfile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customerUser == null)
            {
                return NotFound();
            }

            return View(customerUser);
        }

        // POST: Admin/CustomerUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _logicCustomerUser.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerUserExists(int id)
        {
            return _context.CustomerUsers.Any(e => e.Id == id);
        }

        /// <summary>
        /// Se encarga de retornar los singlePermission para el customerUserId
        /// </summary>
        /// <param name="customerUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetSinglePermissionsByUser(int customerUserId)
        {
            var customerUser = await _context.CustomerUsers.FindAsync(customerUserId);
            var singlePermissionsTitles = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                                            .Where(x => x.CustomerUserId == customerUserId)
                                            .Select(x => x.SinglePermission.Title).ToListAsync();
            return Json(new { customerUser, singlePermissionsTitles });
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmail(string email)
        {
            var customerUser = await _logicCustomerUser.All()
                                        .Where(x => x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(customerUser == null); // is unique
        }

        [HttpGet]
        public async Task<JsonResult> UniqueEmailUpdate(int id, string email)
        {
            var customerUser = await _logicCustomerUser.All()
                                        .Where(x => x.Id != id && x.Email.ToLower() == email.ToLower())
                                        .FirstOrDefaultAsync();
            return Json(customerUser == null); // is unique
        }

        /// <summary>
        /// Se encarga de retornar un true o false dependiendo de sí
        /// ya existe un customerAdmin para el customerInfo seleccionado
        /// </summary>
        /// <returns>bool</returns>
        [HttpGet]
        public async Task<JsonResult> CustomerAdminExistsByCustomerInfo(int customerInfoId)
        {
            var customerAdmin = await _logicCustomerUser.All()
                                                .Where(x => x.CustomerInfoId.Equals(customerInfoId) && x.IsRoot)
                                                .FirstOrDefaultAsync();
            return Json(customerAdmin != null);
        }

        /// <summary>
        /// Se encarga de retornar un true o false dependiendo de sí
        /// ya existe un customerAdmin para el customerInfo seleccionado y
        /// que el customerUser que se quiere editar no sea el propio que tiene el rol administrador
        /// </summary>
        /// <returns>bool</returns>
        [HttpGet]
        public async Task<JsonResult> CustomerAdminExistsByCustomerInfoUpdate(int customerInfoId, int customerUserId)
        {
            var customerAdmin = await _logicCustomerUser.All()
                                                .Where(x => x.CustomerInfoId.Equals(customerInfoId) && x.IsRoot)
                                                .FirstOrDefaultAsync();
            return Json(customerAdmin != null && customerAdmin.Id != customerUserId);
        }

        /// <summary>
        /// Obtener el distributorInfoId, cuando entra un customerUser administrador, 
        /// se obtiene el valor desde el customerInfo
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetDistributorInfoId()
        {
            int distributorInfoId = 0;
            if (User.IsInRole("Admin"))
            {
                distributorInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
            }
            else if (User.IsInRole("CustomerAdmin"))
            {
                int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
                var customerInfo = await _context.CustomerInfos.FindAsync(customerInfoId);
                distributorInfoId = customerInfo.DistributorInfoId;
            }
            return distributorInfoId;
        }

        /// <summary>
        /// Obtiene los clientes (customerInfo) por distribuidor
        /// </summary>
        /// <param name="distributorInfoId"></param>
        /// <returns></returns>
        private IQueryable<CustomerInfo> GetCustomerInfosByDist(int distributorInfoId)
        {
            // Consulta de los customerInfo por distribuidor
            return _context.CustomerInfos.Where(x => x.DistributorInfoId.Equals(distributorInfoId));
        }

        /// <summary>
        /// Verifica si el cliente (customerInfo) tiene o no un usuario administrador ya creado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CustomerInfoWithUserAdmin(int customerInfoId)
        {
            // El usuario administrador no necesita esta verificación
            if (User.IsInRole("CustomerAdmin"))
            {
                return false;
            }
            var customerAdmin = _context.CustomerUsers
                                .Where(x => x.CustomerInfoId == customerInfoId && x.IsRoot).FirstOrDefault();
            return customerAdmin != null;
        }

        /*
         * Se encarga de determinar si el distributorUser autenticado tiene acceso a al controlador/vistas
        */
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
