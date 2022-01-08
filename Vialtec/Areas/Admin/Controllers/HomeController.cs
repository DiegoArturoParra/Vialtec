using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vialtec.Models;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Listado de rutas para el Home
            var routes = new List<Route> {
                new Route {
                    Title = "Clientes",
                    Controller = "CustomerInfos",
                    Icon = "fas fa-user-tie"
                },
                new Route {
                    Title = "Usuarios",
                    Controller = "CustomerUsers",
                    Icon = "fas fa-users"
                },
                new Route {
                    Title = "Dispositivos",
                    Controller = "Devices",
                    Icon = "fas fa-bahai"
                },
                new Route
                {
                    Title = "Eventos",
                    Controller = "CustomerModelEvents",
                    Icon = "fas fa-burn"
                },
                new Route
                {
                    Title = "Clientes-Precomandos",
                    Controller = "PrecommandCustomerNames",
                    Icon = "fab fa-black-tie"
                }
            };
            ViewData["routes"] = routes;
            return View();
        }
    }
}