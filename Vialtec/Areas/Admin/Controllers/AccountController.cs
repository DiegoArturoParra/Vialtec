using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utilitarios;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LAccountAdmin _logicAccountAdmin;

        public AccountController(VialtecContext context)
        {
            _context = context;
            _logicAccountAdmin = new LAccountAdmin(context);
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
        public async Task<IActionResult> Login([Bind("Email,PassKey")] DistributorUser distributorUser)
        {
            var model = await _logicAccountAdmin.Login(distributorUser.Email, distributorUser.PassKey);
            if (model != null)
            {
                string role = "Admin";
                // Create the identity for the user admin. Agregar Claims para consultar cuando se requiera
                var identity = new ClaimsIdentity(new[] {
                    new Claim("distributorUserId", model.Id.ToString()),
                    new Claim("distributorInfoId", model.DistributorInfoId.ToString()),
                    new Claim("distributorUser", JsonConvert.SerializeObject(model)),
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(ClaimTypes.Role, role)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);
                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }
            return View(distributorUser);
        }

        /*
         * Ajax
         * Validar si los parámetros de autenticación son validos para un distributorUser
         */
        public async Task<JsonResult> ValidateUser(string email, string passKey)
        {
            try
            {
                var distributorUser = await _logicAccountAdmin.Login(email, passKey);
                bool valid = distributorUser != null && distributorUser.DistributorInfo.Active;
                string message = string.Empty;
                if (!valid)
                {
                    if (distributorUser != null && !distributorUser.DistributorInfo.Active)
                    {
                        message = $"El distribuidor <<{distributorUser.DistributorInfo.Title}>> ha sido deshabilitado";
                    }
                    else
                    {
                        message = "Usuario y/o contraseña son incorrectos";
                    }
                }
                return Json(new { valid, message });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        /*
         * Se encarga de enviar el correo para recuperación de contraseña del distributorUser
         */
        [HttpPost]
        public IActionResult PasswordRecovery(string email)
        {
            try
            {
                var logicAccount = new LAccountAdmin();
                string guid = $"{Guid.NewGuid().ToString()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                // Enviar token al correo del Email ingresado
                logicAccount.SendEmailToken(email, guid);
                ViewData["guid"] = guid;
                ViewData["email"] = email;
                // Agregar token a Session
                HttpContext.Session.SetString("guid", guid);
            }
            catch (Exception)
            {
                return View();
            }
            return View();
        }

        /*
         * Se encarga de verificar que el token ingresado sea valid
         * y después cambiar la contraseña
         */
        [HttpPost]
        public async Task<JsonResult> VerifyToken(string email, string token, string newPassword)
        {
            string tokenSession = HttpContext.Session.GetString("guid");
            if (tokenSession == token.Trim())
            {
                try
                {
                    var distributorUser = _context.DistributorUsers.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefault();
                    distributorUser.PassKey = new LAccountAdmin().MD5Hash(newPassword);
                    _context.DistributorUsers.Update(distributorUser);
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
         * Se encarga de consultar si el email existe (distributorUsers)
         */
        [HttpGet]
        public JsonResult EmailExists(string email)
        {
            var distributorUser = _context.DistributorUsers.Where(x => x.Email == email).FirstOrDefault();
            return Json(distributorUser != null);
        }

        /*
         * Cerrar sesión
         */
        [Authorize(Roles = "Admin,CustomerAdmin")]
        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (User.IsInRole("CustomerAdmin"))
            {
                return Redirect("/");
            }
            return RedirectToAction("Login", "Account");
        }
    }
}