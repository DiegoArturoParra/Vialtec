using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    public class HomeController : Controller
    {
        private readonly VialtecContext _context;

        public HomeController(VialtecContext context)
        {
            _context = context;
        }

        /*
         * Se encarga de redireccionar al Home de cada ROL
         */
        [Authorize(Roles = "SuperAdmin,Admin,Customer,CustomerAdmin")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { Area = "Admin" });
            } else if (User.IsInRole("SuperAdmin")){
                return RedirectToAction("Index", "Home", new { Area = "SuperAdmin" });
            }
            //ViewData["base64img"] = HttpContext.Session.GetString("base64img");
            int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
            var customerInfo = await _context.CustomerInfos.FindAsync(customerInfoId);
            ViewData["base64img"] = customerInfo.LogoBase64;
            return View();
        }

        /*
         * Se encarga de mostrar la página de errores
         */
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
