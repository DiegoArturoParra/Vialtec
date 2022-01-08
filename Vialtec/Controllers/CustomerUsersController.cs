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
using System.Security.Claims;
using Logica;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class CustomerUsersController : Controller
    {
        private readonly VialtecContext _context;

        public CustomerUsersController(VialtecContext context)
        {
            _context = context;
        }

        // GET: CustomerUsers/Details/5
        public async Task<IActionResult> Details()
        {
            // Obtener el customerUserId de los Claims del login
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener el customerUser object
            var customerUser = await _context.CustomerUsers
                                    .Include(x => x.CustomerInfo)
                                    .Where(x => x.Id.Equals(customerUserId))
                                    .FirstOrDefaultAsync();
            return View(customerUser);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int customerUserId)
        {
            int customerUserIdClaim = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            if (customerUserIdClaim != customerUserId)
            {
                return NotFound();
            }
            var customer = await _context.CustomerUsers.FindAsync(customerUserId);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        /*
         * Se encarga de cambiar la contraseña del customerUser
         */
        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind("Id,PassKey")] CustomerUser customerUser)
        {
            var customer = await _context.CustomerUsers.FindAsync(customerUser.Id);
            if (customer == null)
            {
                return NotFound();
            }
            customer.PassKey = new LCustomerUser().MD5Hash(customerUser.PassKey);
            _context.CustomerUsers.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details");
        }

        /*
         * Es utilizada por una petición ajax para saber si la contraseña que se ingresa es igual a la actual
         */
        [HttpGet]
        public JsonResult ComparePassword(string passkey, string currentPassword)
        {
            var passkeyMD5 = new LCustomerUser().MD5Hash(passkey);
            return Json(passkeyMD5 == currentPassword);
        }
    }
}
