using System;
using Datos;
using Logica;
using Api.Responses;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly LProject _logicProject;
        private readonly IHttpContextAccessor _haccess;

        public ProjectsController(VialtecContext context, IHttpContextAccessor haccess)
        {
            _haccess = haccess;
            _logicProject = new LProject(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);

            // Obtener los proyectos para el usuario
            var projects = await _logicProject.All()
                                 .Include(x => x.Subprojects)
                                 .Where(x => x.CustomerInfoId == user.CustomerInfoId)
                                 .OrderBy(x => x.Id)
                                 .ToListAsync();
            var response = new ApiResponse<IEnumerable<Project>>(projects);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> FindById(int id)
        {
            var project = await _logicProject.Find(id);
            if (project == null)
            {
                return NotFound();
            }
            var response = new ApiResponse<Project>(project);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            // Obtener usuario de la petición
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);

            // Rellenar campos faltantes
            project.CustomerInfoId = user.CustomerInfoId;
            project.UserAlias = user.Email;
            project.CreatedDate = DateTime.Now;
            project.SyncSourceId = 1;

            var res = await _logicProject.Create(project);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }
            var res = await _logicProject.Update(project);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _logicProject.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            var res = await _logicProject.Delete(id);
            var response = new ApiResponse<bool>(res > 0);
            return Ok(response);
        }
    }
}