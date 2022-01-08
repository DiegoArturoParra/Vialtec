using Api.Responses;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilitarios;

namespace Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentsController : ControllerBase
    {
        private readonly LDevice _logicDevice;
        private readonly LEquipment _logicEquipment;
        private readonly IHttpContextAccessor _haccess;

        public EquipmentsController(VialtecContext context, IHttpContextAccessor haccess)
        {
            _haccess = haccess;
            _logicDevice = new LDevice(context);
            _logicEquipment = new LEquipment(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetByNetworkIdentifier(string networkIdentifier)
        {
            // Buscar device
            var device = await _logicDevice.All().FirstOrDefaultAsync(x => x.NetworkIdentifier == networkIdentifier);
            if (device == null)
            {
                return NotFound();
            }
            // Buscar equipment
            var equipment = await _logicEquipment.All().FirstOrDefaultAsync(x => x.DeviceId == device.Id);
            if (equipment == null)
            {
                return NotFound();
            }
            // Responder
            var response = new ApiResponse<Equipment>(equipment);
            return Ok(response);
        }

        [HttpGet]
        [Route("byCategory")]
        public async Task<IActionResult> GetEquipments(int? categoryId) // 1
        {
            if (categoryId == null)
            {
                return BadRequest();
            }
            // Cliente ID
            int customerInfoId = GetCustomerInfoId();
            var equipments = await _logicEquipment.All().Include(x => x.EquipmentGroup).Include(x => x.Device).ThenInclude(x => x.Model)
                            .Where(x => x.EquipmentGroup.CustomerInfoId == customerInfoId && x.Device.Model.CategoryId == categoryId)
                            .Select(x => new Equipment
                            {
                                Id = x.Id,
                                EquipmentAlias = x.EquipmentAlias,
                                DeviceId = x.DeviceId,
                                Device = new Device {
                                    Id = x.Device.Id,
                                    AssetSerial = x.Device.AssetSerial,
                                    NetworkIdentifier = x.Device.NetworkIdentifier,
                                    ModelId = x.Device.ModelId,
                                    Model = x.Device.Model,
                                    BluetoothInfo = x.Device.BluetoothInfo,
                                    DevPass = x.Device.DevPass
                                },
                                Description = x.Description,
                                EquipmentGroupId = x.EquipmentGroupId
                            }).ToListAsync();

            var response = new ApiResponse<IEnumerable<Equipment>>(equipments);
            return Ok(response);
        }

        private int GetCustomerInfoId()
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);
            return user.CustomerInfoId;
        }
    }
}