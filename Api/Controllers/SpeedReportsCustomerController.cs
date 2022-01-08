using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Responses;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Utilitarios;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class SpeedReportsCustomerController : ControllerBase
    {
        private readonly LSpeedReportCustomer _logicSpeedReportCustomer;
        private readonly IHttpContextAccessor _haccess;

        public SpeedReportsCustomerController(VialtecContext context, IHttpContextAccessor haccess)
        {
            _haccess = haccess;
            _logicSpeedReportCustomer = new LSpeedReportCustomer(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpeedReportsByCustomer()
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);
            // Obtener los speed report por cliente
            var speedReportsCustomer = await _logicSpeedReportCustomer.All()
                                        .Where(x => x.CustomerInfoId == user.CustomerInfoId).OrderBy(x => x.Id).ToListAsync();
            var response = new ApiResponse<IEnumerable<SpeedReportCustomer>>(speedReportsCustomer);
            return Ok(response);
        }
    }
}