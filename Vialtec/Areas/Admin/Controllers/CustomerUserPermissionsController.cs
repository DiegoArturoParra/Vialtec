using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilitarios;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "CustomerAdmin")]
    public class CustomerUserPermissionsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCustomerUserPermission _logicCustomerUserPermission;

        public CustomerUserPermissionsController(VialtecContext context)
        {
            _context = context;
            _logicCustomerUserPermission = new LCustomerUserPermission(context);
        }

        public async Task<IActionResult> Index(int? customerInfoId)
        {
            int distributorInfoId = await GetDistributorInfoId();

            // ViewDatas
            ViewData["customerInfoId"] = customerInfoId;
            ViewData["customerInfos"] = await _context.CustomerInfos
                                            .Where(x => x.DistributorInfoId == distributorInfoId)
                                            .ToListAsync();

            // Usuarios por distributor info id
            var customerUsers = _context.CustomerUsers.Include(x => x.CustomerInfo)
                                .Where(x => x.CustomerInfo.DistributorInfoId == distributorInfoId);

            // Si el que solicita la información es un usuario administrador
            if (User.IsInRole("CustomerAdmin"))
            {
                customerUsers = customerUsers.Where(x => x.CustomerInfoId == GetCustomerInfoId());
                // Excluir al usuario administrador del listado de usuarios
                customerUsers = customerUsers.Except(customerUsers.Where(x => x.Id == GetCustomerUserId()));
            }

            // Filtrar por customerInfoId
            if (customerInfoId != null && customerInfoId != -1)
            {
                customerUsers = customerUsers.Where(x => x.CustomerInfoId == customerInfoId);
            }

            return View(await customerUsers.OrderBy(x => x.CustomerInfoId).ToListAsync());
        }

        public async Task<IActionResult> Edit(int? customerUserId)
        {
            if (customerUserId == null)
            {
                return NotFound();
            }

            var customerUser = await _context.CustomerUsers.Include(x => x.CustomerInfo)
                                    .FirstOrDefaultAsync(x => x.Id == customerUserId);

            // Verificar que el usuario exista
            if (customerUser == null)
            {
                return NotFound();
            }
            
            // Verificar que el customerUserId corresponda a uno de los usuarios del actual customerInfo administrado
            if (customerUser.CustomerInfoId != GetCustomerInfoId())
            {
                return NotFound();
            }

            ViewData["permissions"] = await _context.SinglePermissions.OrderBy(x => x.Id).ToListAsync();
            return View(customerUser);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int customerUserId, string permissionIDsStr)
        {
            // Remover todos los precomandos asignados para el customer info
            var itemsToRemove = await _logicCustomerUserPermission.All().Where(x => x.CustomerUserId == customerUserId).ToListAsync();
            if (itemsToRemove.Count != 0)
                await _logicCustomerUserPermission.DeleteRange(itemsToRemove);

            if (!string.IsNullOrEmpty(permissionIDsStr))
            {
                var permissionIDs = permissionIDsStr.Split("#").Select(x => Convert.ToInt32(x));
                // Crear los objetos CustomerUserPermission
                var customerUserPermissions = new List<CustomerUserPermission>();
                foreach (var id in permissionIDs)
                {
                    customerUserPermissions.Add(new CustomerUserPermission
                    {
                        CustomerUserId = customerUserId,
                        SinglePermissionId = id
                    });
                }
                // Almacenar los precommands by customer actualizados
                await _logicCustomerUserPermission.CreateRange(customerUserPermissions);
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Se obtienen los SinglePermission asignados el CustomerUser 
        /// </summary>
        /// <param name="customerUserId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetPermissionsByCustomerUserId(int customerUserId)
        {
            var singlePermissions = _logicCustomerUserPermission.All()
                                        .Where(x => x.CustomerUserId == customerUserId)
                                        .Select(x => x.SinglePermission);
            return Json(new { customerUserId, permissions = await singlePermissions.ToListAsync() });
        }

        /// <summary>
        /// Obtener el distributorInfoId, cuando entra un customerUser administrador, 
        /// se obtiene el valor desde el customerInfo
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetDistributorInfoId()
        {
            int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity)
                                .FindFirst("customerInfoId").Value);
            var customerInfo = await _context.CustomerInfos.FindAsync(customerInfoId);
            return customerInfo.DistributorInfoId;
        }

        /// <summary>
        /// Obtienen el customerInfoId de los claims
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity)
                    .FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Obtener el customerUserId del actual usuario (administrador)
        /// </summary>
        /// <returns></returns>
        private int GetCustomerUserId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity)
                          .FindFirst("customerUserId").Value);
        }
    }
}