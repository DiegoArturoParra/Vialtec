using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Utilitarios;

namespace Vialtec.Controllers
{
    public class AccountController : Controller
    {

        private LAccount logicAccount;
        private VialtecContext _context;

        public AccountController(VialtecContext context)
        {
            _context = context;
            logicAccount = new LAccount(context);
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si el usuario ya está autenticado, se redirige al Home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,PassKey")] CustomerUser customerUser)
        {
            var customerUserLogin = await logicAccount.Login(customerUser.Email, customerUser.PassKey);
            if (customerUserLogin != null)
            {
                // Definir ROL de cliente
                string role = customerUserLogin.IsRoot ? "CustomerAdmin" : "Customer";
                // Agregar Claims para consultar cuando se requieran en los demás controllers
                var identity = new ClaimsIdentity(new[] {
                    new Claim("customerUserId", customerUserLogin.Id.ToString()),
                    new Claim("customerInfoId", customerUserLogin.CustomerInfoId.ToString()),
                    new Claim(ClaimTypes.Name, customerUserLogin.Email),
                    new Claim(ClaimTypes.Role, role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                // SignIn .Net Core
                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                // Redireccionar al Home
                return RedirectToAction("Index", "Home");
            }
            return View(customerUser);
        }

        // Utilizado para una petición AJAX en el login para verificar que el customerUser exista
        public async Task<bool> ValidateUser(string email, string passKey)
        {
            try
            {
                var customerUser = await logicAccount.Login(email, passKey);
                return customerUser != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Cerrar Sesión
        [Authorize(Roles = "Customer,CustomerAdmin")]
        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        /*
         * Se encarga del direccionamiento cuando el usuario accede a una página denegada para ROL
         */
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        /*
         * Se encarga de la petición para recuperar contraseña
         */
        [HttpPost]
        public IActionResult PasswordRecovery(string email)
        {
            try
            {
                var logicAccount = new LAccount();
                string guid = $"{Guid.NewGuid().ToString()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                // Enviar token al correo del Email ingresado
                logicAccount.SendEmailToken(email, guid);
                ViewData["guid"] = guid;
                ViewData["email"] = email;
                // Agregar token a Session
                HttpContext.Session.SetString("guid", guid);
            }
            catch (System.Net.Mail.SmtpException)
            {
                return View();
            }
            return View();
        }

        /*
         * Se encarga de verificar si el token que ingreso el usuario coincide con el almacenado en sesión
         */
        [HttpPost]
        public async Task<JsonResult> VerifyToken(string email, string token, string newPassword)
        {
            string tokenSession = HttpContext.Session.GetString("guid");
            if (tokenSession == token.Trim())
            {
                try
                {
                    var customerUser = _context.CustomerUsers.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefault();
                    customerUser.PassKey = new LAccount().MD5Hash(newPassword);
                    _context.CustomerUsers.Update(customerUser);
                    await _context.SaveChangesAsync();
                    return Json(true);
                }
                catch (Exception)
                {
                    return Json(false);
                }
            }
            return Json(false);
        }

        /*
         * Se encarga de verificar si el email ingreso por el usuario existe
         */
        [HttpGet]
        public JsonResult EmailExists(string email)
        {
            var customerUser = _context.CustomerUsers.Where(x => x.Email == email).FirstOrDefault();
            return Json(customerUser != null);
        }
    }
}