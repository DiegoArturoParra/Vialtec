using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vialtec.Models;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Rutas del Home
            var routes = new List<Route> {
                new Route {
                    Title = "Distribuidores",
                    Controller = "DistributorInfos",
                    Icon = "fas fa-user-tie"
                },
                new Route {
                    Title = "Usuarios Dist.",
                    Controller = "DistributorUsers",
                    Icon = "fas fa-user-astronaut"
                },
                new Route {
                    Title = "Dispositivos",
                    Controller = "Devices",
                    Icon = "fas fa-bahai"
                },
                new Route {
                    Title = "Perfiles de Seg.",
                    Controller = "SecurityProfileDists",
                    Icon = "fas fa-shield-alt"
                },
                new Route
                {
                    Title = "Categorías",
                    Controller = "Categories",
                    Icon = "fas fa-circle-notch"
                },
                new Route
                {
                    Title = "Modelos",
                    Controller = "Models",
                    Icon = "fas fa-meteor"
                },
                new Route
                {
                    Title = "Eventos",
                    Controller = "Events",
                    Icon = "fas fa-bolt"
                },
                new Route
                {
                    Title = "Modelos-Eventos",
                    Controller = "ModelEvents",
                    Icon = "fas fa-atom"
                },
                new Route
                {
                    Title = "Precomandos",
                    Controller = "Precommands",
                    Icon = "fas fa-terminal"
                },
                new Route {
                    Title = "Telegram bots",
                    Controller = "TelegramBots",
                    Icon = "fas fa-robot"
                }
            };
            ViewData["routes"] = routes;
            return View();
        }
    }
}