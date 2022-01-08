using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Utilitarios;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class SendCommandController : Controller
    {
        private readonly VialtecContext _context;

        public SendCommandController(VialtecContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns>View</returns>
        public async Task<IActionResult> Index()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var equipmentGroups = _context.EquipmentGroups.Where(x => x.CustomerInfoId == GetCustomerInfoId());
            ViewData["EquipmentGroupId"] = new SelectList(equipmentGroups, "Id", "Title");
            return View();
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        private int GetCustomeUserId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
        }

        /// <summary>
        /// Método utilizado para obtener los equipments por equipmentGroup id enviado en la petición AJAX
        /// </summary>
        /// <param name="equipmentGroupId"></param>
        /// <returns>Json de equipments</returns>
        [HttpGet]
        public async Task<JsonResult> GetEquipmentsByEquipmentGroupId(int equipmentGroupId)
        {
            var equipments = _context.Equipments.Where(x => x.EquipmentGroupId == equipmentGroupId);
            return Json(await equipments.ToListAsync());
        }

        /// <summary>
        /// Método utilizado para obtener los precommands para el modelId enviado como parámetros
        /// teniendo en cuenta el nombre personalizado dado por los customerInfo
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <returns>Json de precommands</returns>
        [HttpGet]
        public async Task<JsonResult> GetPrecommandsByEquipmentId(int equipmentId)
        {
            // Obtener el modelId del equipment
            int modelId = await _context.Equipments.Include(x => x.Device)
                                .Where(x => x.Id == equipmentId)
                                .Select(x => x.Device.ModelId).FirstOrDefaultAsync();

            // Obtener precomandos con el nombre personalizados filtrando por el cliente y modelo
            var precommandCustomerNames = _context.PrecommandCustomerNames
                                            .Include(x => x.Precommand)
                                            .Where(x => x.CustomerInfoId == GetCustomerInfoId() && x.Precommand.ModelId == modelId);

            #region "USUARIO ADMINISTRADOR"
            // Si el usuario que solicita la información es el usuario administrador, se retornan todos los precommandCustomerNames
            // Sin tener que filtrar por los precommandByUser
            if (User.IsInRole("CustomerAdmin"))
            {
                return Json(await precommandCustomerNames.ToListAsync());
            }
            #endregion

            #region "USUARIO NORMAL"
            // Obtener precomandos también por usuario y modelo
            var precommandByUser = _context.PrecommandByUsers.Include(x => x.Precommand)
                                   .Where(x => x.CustomerUserId == GetCustomeUserId() && x.Precommand.ModelId == modelId);

            // Join entre ambas consultas para obtener los precommandCustomerNames
            var query = from x in precommandCustomerNames
                        join z in precommandByUser on x.PrecommandId equals z.PrecommandId
                        select x;

            return Json(await query.ToListAsync());
            #endregion
        }

        /// <summary>
        /// Almacenar la información del comando
        /// </summary>
        /// <param name="precommandId"></param>
        /// <param name="equipmentId"></param>
        /// <returns>id del comando recien creado o -1 para indicar que ha ocurrido un error</returns>
        [HttpGet]
        public async Task<JsonResult> SaveCommand(int precommandId, int equipmentId)
        {
            var precommand = await _context.Precommands.Where(x => x.Id == precommandId).FirstOrDefaultAsync();
            var equipment = await _context.Equipments.Include(x => x.Device).ThenInclude(x => x.Model)
                                    .Where(x => x.Id == equipmentId).FirstOrDefaultAsync();
            var rnd = new Random();
            string randomNumStr = rnd.Next().ToString();
            var command = new Command
            {
                CustomId = randomNumStr,
                DeviceIdentifier = equipment.Device.NetworkIdentifier,
                CommandData = precommand.CommandData,
                ExpectedAck = precommand.ExpectedAck,
                EncodingTypeId = equipment.Device.Model.EncodingTypeId,
                DeliveryState = 0,
                CreationDT = DateTime.Now,
                ReceivedInfo = string.Empty
            };
            try
            {
                _context.Commands.Add(command);
                await _context.SaveChangesAsync();
                return Json(command.Id);
            }
            catch(Exception)
            {
                return Json(-1);
            }
        }

        /// <summary>
        /// Obtener el objeto Model por el equipmentId seleccionado
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult>GetModelByEquipmentId(int equipmentId)
        {
            // Encontrar model por equipmentId
            Model model = await _context.Equipments.Include(x => x.Device).ThenInclude(x => x.Model)
                                .Where(x => x.Id == equipmentId)
                                .Select(x => x.Device.Model).FirstOrDefaultAsync();
            return Json(model);
        }

        /// <summary>
        /// Consultar el estado del comando
        /// </summary>
        /// <param name="commandId"></param>
        /// <returns>DeliveryState del comando</returns>
        [HttpGet]
        public async Task<JsonResult> ConsultCommandDeliveryState(int commandId)
        {
            int deliveryState = await _context.Commands.Where(x => x.Id == commandId)
                                       .Select(x => x.DeliveryState).FirstOrDefaultAsync();
            return Json(deliveryState);
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains("GestionRemota");
        }
    }
}