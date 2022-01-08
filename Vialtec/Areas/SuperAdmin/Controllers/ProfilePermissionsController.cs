using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilitarios;

namespace Vialtec.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ProfilePermissionsController : Controller
    {
        private readonly VialtecContext _context;

        public ProfilePermissionsController(VialtecContext context)
        {
            _context = context;
        }

        /*
         * Se encarga de mostrar los ProfilePermission de un securityProfile
         */
        [HttpGet]
        // GET: Admin/ProfilePermissions/ModifyProfilePermission
        public async Task<IActionResult> ModifyProfilePermission(int? securityProfileDistId)
        {
            if (securityProfileDistId == null)
            {
                return NotFound();
            }
            // Buscar el securityProfile (para distribuidor)
            var securityProfileDist = await _context.SecurityProfileDists
                                        .Include(x => x.DistributorInfo)
                                        .Include(x => x.ProfilePermissionsDists).ThenInclude(x => x.SinglePermissionDist)
                                        .FirstOrDefaultAsync(x => x.Id == securityProfileDistId);
            if (securityProfileDist == null)
            {
                return NotFound();
            }
            // Ordernar los profilePermissions del securityProfile
            securityProfileDist.ProfilePermissionsDists = securityProfileDist.ProfilePermissionsDists.OrderBy(x => x.Id).ToList();
            // Join de los permisos que tiene el securityProfile y todos los permisos existentes
            var singlePermissionsNotAssigned = (from x in securityProfileDist.ProfilePermissionsDists.Select(x => x.SinglePermissionDist)
                                                join z in _context.SinglePermissionDists on x.Id equals z.Id
                                                select z).ToList();
            // Enviamos a la vista los permissos que aún no han sido asignados
            ViewData["singlePermissionsNotAssigned"] = _context.SinglePermissionDists.Except(singlePermissionsNotAssigned)
                                                                                 .OrderBy(x => x.Id).ToList();
            return View(securityProfileDist);
        }

        /*
         * Se encarga de modificar los ProfilePermission de un securityProfile
         */
        [HttpPost]
        public async Task<JsonResult> ModifyProfilePermission(int securityProfileDistId, string singlePermissionDistIdsStr)
        {
            try
            {
                // Eliminar todos los ProfilePermissions actuales del securityProfile para almacenar los nuevos
                _context.ProfilePermissionDists
                        .RemoveRange(_context.ProfilePermissionDists.Where(x => x.SecurityProfileDistId == securityProfileDistId));
                await _context.SaveChangesAsync();

                // si no es null o vacío el listado de SinglePermissionsDists
                if (!string.IsNullOrEmpty(singlePermissionDistIdsStr) && !singlePermissionDistIdsStr.Contains("undefined"))
                {
                    singlePermissionDistIdsStr = singlePermissionDistIdsStr.TrimEnd(',');
                    // Agregar los ids de los nuevos singlePermission para el securityProfile
                    var singlePermissionIds = new List<int>();
                    foreach (string id in singlePermissionDistIdsStr.Split(','))
                    {
                        singlePermissionIds.Add(Convert.ToInt32(id));
                    }
                    // Crear los registros ProfilePermissions
                    var profilesPermissionsDists = new List<ProfilePermissionDist>();
                    foreach (int id in singlePermissionIds)
                    {
                        var pp = new ProfilePermissionDist
                        {
                            SecurityProfileDistId = securityProfileDistId,
                            SinglePermissionDistId = id
                        };
                        profilesPermissionsDists.Add(pp);
                    }
                    // Almacenar profilePermissions
                    _context.ProfilePermissionDists.AddRange(profilesPermissionsDists);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                return Json(false);
            }
            return Json(true);
        }

        /*
         * Se encarga de retornar un SinglePermission por id
         */
        [HttpGet]
        public async Task<JsonResult> GetSinglePermissionById(int id)
        {
            var permission = await _context.SinglePermissionDists.FindAsync(id);
            return Json(permission);
        }
    }
}