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

namespace Vialtec.Controllers
{
    [Authorize(Roles = "CustomerAdmin")]
    public class PrecommandByUsersController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LPrecommandCustomerName _lPrecommandCustomerName;
        private readonly LPrecommandByUser _lPrecommandByUser;

        public PrecommandByUsersController(VialtecContext context)
        {
            _context = context;
            _lPrecommandCustomerName = new LPrecommandCustomerName(context);
            _lPrecommandByUser = new LPrecommandByUser(context);
        }

        public async Task<IActionResult> Index()
        {
            int customerInfoId = GetCustomerInfoId();
            // Obtener los usuarios filtrados por id del cliente, y exceptuando al usuario administrador
            var customerUsers =  _context.CustomerUsers
                                .Where(x => x.CustomerInfoId == customerInfoId && x.Id != GetCustomerUserId());
            return View(await customerUsers.ToListAsync());
        }

        /// <summary>
        /// Obtener el id del cliente en sesión
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Obtener el id del usuario administrador en sesión
        /// </summary>
        /// <returns></returns>
        private int GetCustomerUserId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
        }

        /// <summary>
        /// Obtener los precomandos asignados a cada usuario
        /// </summary>
        /// <param name="customerUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetPrecommandCustomerNamesByUser(int customerUserId)
        {
            // Obtener precomandos con el nombre personalizados filtrando por el cliente
            var precommandCustomerNames = _context.PrecommandCustomerNames.Where(x => x.CustomerInfoId == GetCustomerInfoId());
            // Obtener precomandos por usuario
            var precommandByUser = _context.PrecommandByUsers.Include(x => x.Precommand)
                                   .Where(x => x.CustomerUserId == customerUserId);
            // Join entre ambas consultas para obtener los precommandCustomerNames
            var query = from x in precommandCustomerNames.Include(x => x.Precommand).ThenInclude(x => x.Model)
                        join z in precommandByUser on x.PrecommandId equals z.PrecommandId
                        select x;
            return Json(new { customerUserId, precommandCustomerNames = await query.ToListAsync() });
        }

        public async Task<IActionResult> Edit(int? customerUserId)
        {
            if (customerUserId == null)
            {
                return NotFound();
            }

            var customerUser = await _context.CustomerUsers.FirstOrDefaultAsync(x => x.Id == customerUserId);
            ViewData["precommandCustomerNames"] = await _context.PrecommandCustomerNames.Include(x => x.Precommand).ThenInclude(x => x.Model)
                                                        .Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                                        .OrderBy(x => x.Precommand.ModelId)
                                                        .ToListAsync();
            return View(customerUser);
        }

        /// <summary>
        /// Editar los precomandos al usuario
        /// </summary>
        /// <param name="customerUserId"></param>
        /// <param name="precommandIDsStr"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(int customerUserId, string precommandIDsStr)
        {
            // Remover todos los precomandos asignados para el customer info
            var itemsToRemove = await _lPrecommandByUser.All().Where(x => x.CustomerUserId == customerUserId).ToListAsync();
            if (itemsToRemove.Count != 0)
                await _lPrecommandByUser.DeleteRange(itemsToRemove);

            if (!string.IsNullOrEmpty(precommandIDsStr))
            {
                var precommandIDs = precommandIDsStr.Split("#").Select(x => Convert.ToInt32(x));
                // Crear los objetos PrecommandByCustomer
                var precommandsByCustomer = new List<PrecommandByUser>();
                foreach (var id in precommandIDs)
                {
                    precommandsByCustomer.Add(new PrecommandByUser
                    {
                        CustomerUserId = customerUserId,
                        PrecommandId = id
                    });
                }
                // Almacenar los precommands by customer actualizados
                await _lPrecommandByUser.CreateRange(precommandsByCustomer);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}