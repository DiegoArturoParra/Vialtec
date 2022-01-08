using Api.Models;
using Api.Responses;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class VehicleTypesController : ControllerBase
    {
        private readonly LVehicleType _logicVehicleType;
        private readonly LCustomVehicleType _logicCustomVehicleType;

        public VehicleTypesController(VialtecContext context)
        {
            _logicVehicleType = new LVehicleType(context);
            _logicCustomVehicleType = new LCustomVehicleType(context);
        }

        [HttpGet]
        [Route("custom")]
        public async Task<IActionResult> GetCustomVehicleTypes(int? speedReportId)
        {
            if (speedReportId == null)
            {
                return BadRequest();
            }
            var query = from ct in _logicVehicleType.All().Where(x => x.Id != -1).OrderBy(x => x.Id)
                        let custom_vehicle_type = _logicCustomVehicleType.All().FirstOrDefault(z => z.VehicleTypeId == ct.Id && z.SpeedReportCustomerId == speedReportId)
                        select new VehicleTypeModel
                        {
                            Id = ct.Id,
                            Title = custom_vehicle_type == null ? ct.Title : custom_vehicle_type.CustomTitle,
                            Img = custom_vehicle_type == null ? "" : custom_vehicle_type.Picture,
                            Personalized = custom_vehicle_type != null
                        };

            var response = new ApiResponse<IEnumerable<VehicleTypeModel>>(await query.ToListAsync());
            return Ok(response);
        }
    }
}