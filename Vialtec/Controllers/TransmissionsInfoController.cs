using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilitarios;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class TransmissionsInfoController : Controller
    {
        private readonly VialtecContext _context;

        public TransmissionsInfoController(VialtecContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? pageNumber, int? equipmentGroupId, int? equipmentId, string customerModelEventIds, string dateInit, string dateFinal)
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Número de registros por página
            int pageSize = 50;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas para los filtros iniciales de la vista
            ViewData["customerInfoId"] = GetCustomerInfoId();
            ViewData["datetime-inicial"] = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewData["datetime-final"] = DateTime.Today.AddDays(1).AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ss");
            ViewData["equipmentGroups"] = _context.EquipmentGroups
                                         .Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();

            // Obtener los registros TransmissionInfo aplicando los filtros enviados por la vista
            // La primera vez que entra no le retornamos data porque se necesitan llenar los filtros
            var query = _context.TransmissionInfos.Where(x => x.Id == -1);

            // Filtro de customer Model Events Ids y rango de fechas
            if (!string.IsNullOrEmpty(customerModelEventIds) && !string.IsNullOrEmpty(dateInit) && !string.IsNullOrEmpty(dateFinal))
            {
                // Obtener el id del modelo por el equipmentId recibido
                var modelIdByEquipment = _context.Equipments.Include(x => x.Device)
                                            .Where(x => x.Id == equipmentId)
                                            .Select(x => x.Device.ModelId)
                                            .FirstOrDefault();
                // Listado de ids de registros cusotmerModelEvents
                var customerModelEventIdsList = customerModelEventIds.Split('#')
                                                .Select(x => Convert.ToInt32(x)).ToList();
                // Obtener los ids de los ModelEvent para la posterior consulta
                var modelEventIds = _context.CustomerModelEvents
                                    .Where(x => customerModelEventIdsList.Contains(x.Id))
                                    .Select(x => x.ModelEventId)
                                    .ToList();
                // Obtener los ids de los Eventos que se quieren consultar
                var eventIds = _context.ModelEvents
                                .Where(x => modelEventIds.Contains(x.Id))
                                .Select(x => x.EventId).ToList();
                // Query de TransmissionsInfo aplicando filtros e inner joins
                query = (from t in _context.TransmissionInfos
                        join m in _context.ModelEvents on t.EventId equals m.EventId
                        join c in _context.CustomerModelEvents on m.Id equals c.ModelEventId
                        where m.ModelId == modelIdByEquipment &&
                        t.EquipmentId == equipmentId &&
                        c.CustomerInfoId == GetCustomerInfoId() &&
                        eventIds.Contains(t.EventId ?? -1) &&
                        t.DeviceDt >= Convert.ToDateTime(dateInit) && t.DeviceDt <= Convert.ToDateTime(dateFinal)
                        orderby t.DeviceDt descending
                        select new TransmissionInfo
                        {
                            Id = t.Id,
                            ServerDt = t.ServerDt,
                            DeviceDt = t.DeviceDt,
                            Latitude = t.Latitude,
                            Longitude = t.Longitude,
                            GpsValid = t.GpsValid,
                            EventAlias = c.Title
                        }).Take(2000);

                // ViewDatas
                ViewData["equipmentId"] = equipmentId;
                ViewData["equipmentGroupId"] = equipmentGroupId;
                ViewData["customerModelEventIds"] = customerModelEventIds;
                ViewData["datetime-inicial"] = dateInit;
                ViewData["datetime-final"] = dateFinal;
            }

            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convertir por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasar el query de EquipmentGroup por el modelo de paginación
            return View(await PaginatedList<TransmissionInfo>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Obtener el id del customerInfo almacenado en los Claims
        /// </summary>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Obtener el registro de transmission para mostrarlo en un minimapa
        /// </summary>
        /// <param name="transmissionID"></param>
        /// <returns>Json</returns>
        public async Task<JsonResult> GetTransmissionById(int transmissionID, string eventAlias)
        {
            var transmission = await _context.TransmissionInfos.Include(x => x.Equipment)
                                     .Where(x => x.Id == transmissionID)
                                     .Select(x => new TransmissionInfo
                                     {
                                         Latitude = x.Latitude,
                                         Longitude = x.Longitude,
                                         DeviceDt = x.DeviceDt,
                                         EventAlias = eventAlias,
                                         Equipment = new Equipment { EquipmentAlias = x.Equipment.EquipmentAlias }
                                     }).FirstOrDefaultAsync();
            return Json(transmission);
        }

        /// <summary>
        /// Obtener los equipments filtrados por su equipmentGroupId
        /// </summary>
        /// <param name="equipmentGroupId"></param>
        /// <returns>Json</returns>
        [HttpGet]
        public JsonResult GetEquipmentsByEquipmentGroupId(int equipmentGroupId)
        {
            var results = _context.Equipments.Where(x => x.EquipmentGroupId == equipmentGroupId);
            return Json(results);
        }

        /// <summary>
        /// Obtener los customerModelEvents por el equipmentId enviado por el filtro
        /// y por el cliente que está en sesión
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns>Json de resultados</returns>
        [HttpGet]
        public async Task<JsonResult> GetCustomerModelEventByEquipmentId(int equipmentId)
        {
            // Obtener los ids de los models que maneja el equipment
            var modelIds = _context.Equipments.Include(x => x.Device).ThenInclude(x => x.Model)
                            .Where(x => x.Id == equipmentId)
                            .Select(x => x.Device.ModelId);
            // Obtener los customerModelEvents del listado de los ids de los modelos
            var results = from c in _context.CustomerModelEvents.Include(x => x.ModelEvent)
                          where modelIds.Contains(c.ModelEvent.ModelId) && c.CustomerInfoId == GetCustomerInfoId()
                          select c;
            return Json(await results.ToListAsync());
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