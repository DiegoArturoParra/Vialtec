using Api.Responses;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReflectivitiesController : ControllerBase
    {
        private readonly LReflectivity _logicReflectivity;

        public ReflectivitiesController(VialtecContext context)
        {
            _logicReflectivity = new LReflectivity(context);
        }

        [HttpPost]
        public async Task<IActionResult> PostReflectivityList([FromQuery] int zoneTime,
            [FromBody] IEnumerable<Reflectivity> data)
        {
            // Asignar fecha UTC y descontar al zoneTime
            var now = DateTime.UtcNow.AddHours(zoneTime);
            data.ToList().ForEach(x => {
                x.ServetDt = now;
                x.NetworkIdentifier = x.DeviceSerial;
                x.RawData = JsonConvert.SerializeObject(x);
            });

            try
            {
                var result = await _logicReflectivity.AddRange(data);
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