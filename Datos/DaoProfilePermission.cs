using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoProfilePermission
    {
        private readonly VialtecContext _context;

        public DaoProfilePermission(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<ProfilePermission> All()
        {
            // Obtener todos los registros ProfilePermission
            var results = from c in _context.ProfilePermissions
                          select c;
            return results;
        }

        public async Task<int> Create(ProfilePermission profilePermission)
        {
            // Crear nuevo registro ProfilePermission
            _context.ProfilePermissions.Add(profilePermission);
            return await _context.SaveChangesAsync();
        }

        public async Task<ProfilePermission> Find(int? id)
        {
            // Buscar registro ProfilePermission
            return await _context.ProfilePermissions.FindAsync(id);
        }

        public async Task<int> Update(ProfilePermission profilePermission)
        {
            // Actualizar registro ProfilePermission
            _context.Update(profilePermission);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro ProfilePermission
            var profilePermission = await _context.ProfilePermissions.FindAsync(id);
            _context.ProfilePermissions.Remove(profilePermission);
            return await _context.SaveChangesAsync();
        }

        public bool RelationshipExists(int? securityProfileId, int? singlePermissionId)
        {
            // Verificar si ya existe una relación securityProfile-SinglePermission
            var query = (from x in _context.ProfilePermissions
                         where x.SecurityProfileId.Equals(securityProfileId) && x.SinglePermissionId.Equals(singlePermissionId)
                         select x).FirstOrDefault();
            return query != null;
        }

        public bool RelationshipExistsUpdate(int? id, int? securityProfileId, int? singlePermissionId)
        {
            /*
             *  Verificar si ya existe una relación securityProfile-SinglePermission
             *  descartando la propia relación ya existente por ser una actualización
             */
            var query = (from x in _context.ProfilePermissions
                         where x.Id != id && x.SecurityProfileId.Equals(securityProfileId) && x.SinglePermissionId.Equals(singlePermissionId)
                         select x).FirstOrDefault();
            return query != null;
        }
    }
}
