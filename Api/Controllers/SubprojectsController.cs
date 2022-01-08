using Datos;
using Logica;
using System;
using Utilitarios;
using System.Linq;
using Api.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubprojectsController : ControllerBase
    {
        private readonly LSubproject _logicSubproject;
        private readonly IHttpContextAccessor _haccess;

        public SubprojectsController(VialtecContext context, IHttpContextAccessor haccess)
        {
            _haccess = haccess;
            _logicSubproject = new LSubproject(context);
        }

        [HttpGet]
        [Route("findByProjectId")]
        public async Task<IActionResult> FindByProjectId(int? projectId)
        {
            if (projectId == null)
            {
                return BadRequest();
            }
            var subprojects = await _logicSubproject.All().Where(x => x.ProjectId == projectId).ToListAsync();
            var response = new ApiResponse<IEnumerable<Subproject>>(subprojects);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);
            // Obtener listado de actividades para el cliente
            var subprojects = await _logicSubproject.All()
                                    .Include(x => x.Project)
                                    .Where(x => x.Project.CustomerInfoId == user.CustomerInfoId)
                                    .OrderBy(x => x.Id)
                                    .Select(x => new Subproject {
                                        Id = x.Id,
                                        ProjectId = x.ProjectId,
                                        Title = x.Title,
                                        Description = x.Description,
                                        CreatedDate = x.CreatedDate
                                    })
                                    .ToListAsync();
            var response = new ApiResponse<IEnumerable<Subproject>>(subprojects);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> FindById(int id)
        {
            var subproject = await _logicSubproject.Find(id);
            if (subproject == null)
            {
                return NotFound();
            }
            var response = new ApiResponse<Subproject>(subproject);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subproject subproject)
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);

            // Rellenar campos faltantes
            subproject.UserAlias = user.Email;
            subproject.CreatedDate = DateTime.Now;
            subproject.SyncSourceId = 1;

            var res = await _logicSubproject.Create(subproject);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Subproject subproject)
        {
            if (id != subproject.Id)
            {
                return BadRequest();
            }
            var res = await _logicSubproject.Update(subproject);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _logicSubproject.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            var res = await _logicSubproject.Delete(id);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(response);
        }
    }
}