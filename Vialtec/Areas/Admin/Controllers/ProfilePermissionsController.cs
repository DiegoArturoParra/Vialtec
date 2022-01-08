using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Datos;
using Utilitarios;
using System.Security.Claims;
using Logica;
using Vialtec.Models;
using Microsoft.AspNetCore.Authorization;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProfilePermissionsController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LProfilePermission _logicProfilePermission;

        public ProfilePermissionsController(VialtecContext context)
        {
            _context = context;
            _logicProfilePermission = new LProfilePermission(context);
        }

        /*
         * Se encarga de modificar los profilePermission (relación entre securityProfileid-singlePermission)
         */
        [HttpGet]
        // GET: Admin/ProfilePermissions/ModifyProfilePermission
        public async Task<IActionResult> ModifyProfilePermission(int? securityProfileId)
        {
            if (securityProfileId == null)
            {
                return NotFound();
            }
            // Buscar el securityProfile
            var securityProfile = await _context.SecurityProfiles
                                        .Include(x => x.CustomerInfo)
                                        .Include(x => x.ProfilePermissions).ThenInclude(x => x.SinglePermission)
                                        .FirstOrDefaultAsync(x => x.Id == securityProfileId);
            if (securityProfile == null)
            {
                return NotFound();
            }
            // Ordernar los ProfilePermissions del SecurityProfile
            securityProfile.ProfilePermissions = securityProfile.ProfilePermissions.OrderBy(x => x.Id).ToList();
            // Join de los permisos que tiene el securityProfile y todos los permisos existentes
            var singlePermissionsNotAssigned = (from x in securityProfile.ProfilePermissions.Select(x => x.SinglePermission)
                                                join z in _context.SinglePermissions on x.Id equals z.Id
                                                select z).ToList();
            // Enviamos a la vista los permissos que aún no han sido asignados
            ViewData["singlePermissionsNotAssigned"] = _context.SinglePermissions.Except(singlePermissionsNotAssigned)
                                                                                 .OrderBy(x => x.Id).ToList();
            return View(securityProfile);
        }

        [HttpPost]
        public async Task<JsonResult> ModifyProfilePermission(int securityProfileId, string singlePermissionIdsStr)
        {
            try
            {
                // Eliminar todos los ProfilePermissions actuales del securityProfile para almacenar los nuevos
                _context.ProfilePermissions.RemoveRange(_context.ProfilePermissions.Where(x => x.SecurityProfileId == securityProfileId));
                await _context.SaveChangesAsync();

                // si no es null o vacío el listado de singlePermissions
                if (!string.IsNullOrEmpty(singlePermissionIdsStr) && !singlePermissionIdsStr.Contains("undefined"))
                {
                    singlePermissionIdsStr = singlePermissionIdsStr.TrimEnd(',');
                    // Agregar los ids de los nuevos singlePermission para el securityProfile
                    var singlePermissionIds = new List<int>();
                    foreach (string id in singlePermissionIdsStr.Split(','))
                    {
                        singlePermissionIds.Add(Convert.ToInt32(id));
                    }
                    // Crear los registros ProfilePermissions
                    var profilesPermissions = new List<ProfilePermission>();
                    foreach (int id in singlePermissionIds)
                    {
                        var pp = new ProfilePermission
                        {
                            SecurityProfileId = securityProfileId,
                            SinglePermissionId = id
                        };
                        profilesPermissions.Add(pp);
                    }
                    // Almacenar profilePermissions
                    _context.ProfilePermissions.AddRange(profilesPermissions);
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception)
            {
                return Json(false);
            }
            return Json(true);
        }

        /*
         * Ajax
         * Se encarga de retornar el registro SinglePermission por id
         */
        [HttpGet]
        public async Task<JsonResult> GetSinglePermissionById(int id)
        {
            var permission = await _context.SinglePermissions.FindAsync(id);
            return Json(permission);
        }
    }
}
