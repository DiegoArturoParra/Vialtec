using System.Security.Claims;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utilitarios;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class AccountController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LAccountSuperAdmin _logicAccountSuperAdmin;

        public AccountController(VialtecContext context)
        {
            _context = context;
            _logicAccountSuperAdmin = new LAccountSuperAdmin(context);
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Email,Password")] SAdmin superAdmin)
        {
            var model = _logicAccountSuperAdmin.Login(superAdmin.Email, superAdmin.Password);
            if (model != null)
            {
                string role = "SuperAdmin";
                //Create the identity for the user admin. Agregar Claims para consultar cuando se requiera
                var identity = new ClaimsIdentity(new[] {
                    new Claim("superAdminId", model.Id.ToString()),
                    new Claim("superAdmin", JsonConvert.SerializeObject(model)),
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(ClaimTypes.Role, role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);
                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }
            return View(superAdmin);
        }

        /*
         * Se encarga de retornar un boolean que determina si el superAdmin existe/es válido
         */
        public bool ValidateUser(string email, string password)
        {
            var superAdmin = _logicAccountSuperAdmin.Login(email, password);
            return superAdmin != null;
        }

        // Cerrar sesión
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}