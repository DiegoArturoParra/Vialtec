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
    [Authorize(Roles = "Admin")]
    public class DistributorUsersController : Controller
    {
        private readonly VialtecContext _context;

        public DistributorUsersController(VialtecContext context)
        {
            _context = context;
        }

        // GET: DistributorUsers/Details/5
        public async Task<IActionResult> Details()
        {
            int distributorUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorUserId").Value);
            var distributorUser = await _context.DistributorUsers
                                    .Include(x => x.DistributorInfo)
                                    .Include(x => x.SecurityProfileDist)
                                    .Where(x => x.Id == distributorUserId)
                                    .FirstOrDefaultAsync();
            return View(distributorUser);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int distributorUserId)
        {
            int distributorUserIdClaim = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorUserId").Value);
            if (distributorUserIdClaim != distributorUserId)
            {
                return NotFound();
            }
            var distributor = await _context.DistributorUsers.FindAsync(distributorUserId);
            if (distributor == null)
            {
                return NotFound();
            }
            return View(distributor);
        }

        /*
         * Se encarga de cambiar la contraseña del DistributorUser
         */
        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind("Id,PassKey")] DistributorUser distributorUser)
        {
            var distributor = await _context.DistributorUsers.FindAsync(distributorUser.Id);
            if (distributor == null)
            {
                return NotFound();
            }
            distributor.PassKey = new LDistributorUser().MD5Hash(distributorUser.PassKey);
            _context.DistributorUsers.Update(distributor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details");
        }

        /*
         * Se encarga de comparar contraseñas, la actual y por la que se quiere cambiar
         */
        [HttpGet]
        public JsonResult ComparePassword(string passkey, string currentPassword)
        {
            var passkeyMD5 = new LDistributorUser().MD5Hash(passkey);
            return Json(passkeyMD5 == currentPassword);
        }
    }
}