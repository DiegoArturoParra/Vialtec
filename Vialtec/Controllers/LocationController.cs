using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class LocationController : Controller
    {
        private VialtecContext _context;

        public LocationController(VialtecContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
            ViewData["equipmentGroups"] = _context.EquipmentGroups.Where(x => x.CustomerInfoId == customerInfoId).ToList();
            return View();
        }

        /*
         * AJAX
         * Se encarga de retornar un listado de equipments filtrado por equipmentGroupId 
        */
        [HttpGet]
        public JsonResult GetEquipmentsByEquipmentGroupId(int equipmentGroupId)
        {
            var equipments = _context.Equipments
                            .Where(x => x.EquipmentGroupId == equipmentGroupId);
            return Json(equipments);
        }

        /*
         * AJAX
         * Se encarga de recibir los filtros de búsqueda para los Equipments y mostrarlos en el mapa
         */
        [HttpGet]
        public JsonResult GetLocationEquipments(string reportType, string equipments, string dateInitComplete, string dateFinalComplete)
        {
            if (reportType == "ubicacion")
            {
                // Obtener los registros Equipments que se quieren consultar y retornar los atributos necesarios para mostrarlos en el mapa
                var results = GetEquipmentLocationsByFilters(equipments)
                                .Select(x => new {
                                    x.Id,
                                    x.LastPositionDt,
                                    x.LastLongitude,
                                    x.LastLatitude,
                                    x.EquipmentAlias,
                                    x.DeviceId
                                });
                return Json(new { reportType = "ubicacion", results });
            } else if (reportType == "ruta")
            {
                // Para el reporte de ruta solamente se puede consultar un Equipment
                int equipmentId = JsonConvert.DeserializeObject<List<EquipmentSimple>>(equipments)[0].Id;
                /*
                 * Obtener los registros Transmissions del Equipment que se quieren consultar y retornar los atributos 
                 * necesarios para mostrar la ruta en el mapa
                 */
                var results = GetTransmissionInfoByEquipmentId(equipmentId, dateInitComplete, dateFinalComplete)
                                .Select(x => new {
                                    x.Latitude,
                                    x.Longitude,
                                    Alias = x.Equipment.EquipmentAlias,
                                    x.DeviceDt
                                });
                return Json(new { reportType = "ruta", results });
            }
            return null;
        }

        /*
         * Se encarga de retornar los registros Equipments según el filtro equipments
         */

        [HttpGet]
        public IQueryable<Utilitarios.Equipment> GetEquipmentLocationsByFilters(string equipments)
        {
            // Consulta de todos los equipments
            var query = from e in _context.Equipments
                        where e.LastPositionDt != null && e.LastLatitude != null && e.LastLongitude != null
                        select e;
            // Obtener un listado de los equipmentsId enviamos en el filtro
            List<int> equipmentIDs = JsonConvert.DeserializeObject<List<EquipmentSimple>>(equipments).Select(x => x.Id).ToList();
            // Consulta que emula CLAUSULE IN en la consulta de todos los equipments
            query = query.Where(x => equipmentIDs.Contains(x.Id));
            return query;
        }

        /*
         * Se encarga de retornar los registros de transmission del equipment seleccionado
         * con una fecha de inicio y final
         */
        [HttpGet]
        public IQueryable<Utilitarios.TransmissionInfo> GetTransmissionInfoByEquipmentId(int equipmentId, string dateInitComplete, string dateFinalComplete)
        {
            // Formato de fecha inicial y final
            DateTime fechaInicial = DateTime.ParseExact(dateInitComplete, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime fechaFinal = DateTime.ParseExact(dateFinalComplete, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            // Consulta de TransmissionInfo filtrada por el Equipment del filtro
            var query = from e in _context.TransmissionInfos.Include(x => x.Equipment)
                        where e.Latitude != null && e.Longitude != null && e.EquipmentId == equipmentId
                        && e.DeviceDt >= fechaInicial && e.DeviceDt <= fechaFinal
                        orderby e.DeviceDt
                        select e;
            return query;
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener nombre del controlador
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains(nameController);
        }
    }
}