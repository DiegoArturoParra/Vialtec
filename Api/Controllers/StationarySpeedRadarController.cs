using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Responses;
using Utilitarios;
using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;

namespace Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class StationarySpeedRadarController : ControllerBase
    {
        private readonly LDevice _logicDevice;
        private readonly LEquipment _logicEquipment;
        private readonly LStationarySpeedRadar _logicStationarySpeedRadar;

        public StationarySpeedRadarController(VialtecContext context)
        {
            _logicDevice = new LDevice(context);
            _logicEquipment = new LEquipment(context);
            _logicStationarySpeedRadar = new LStationarySpeedRadar(context);
        }

        [HttpPost]
        public async Task<IActionResult> PostStationarySpeedRadarList([FromQuery] int zoneTime, [FromBody] IEnumerable<StationarySpeedRadar> data)
        {
            // Asignar fecha UTC y descontar al zoneTime
            var now = DateTime.UtcNow.AddHours(zoneTime);
            data.ToList().ForEach(x => x.ServerDt = now);

            try
            {
                var result = await _logicStationarySpeedRadar.AddRange(data);
                var response = new ApiResponse<bool>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {

                var response = new ApiResponse<string>(ex.Message);
                return BadRequest(response);
            }
        }
    }
}