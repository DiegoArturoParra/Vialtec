using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LProfilePermission
    {
        private DaoProfilePermission daoProfilePermission;

        public LProfilePermission(VialtecContext context)
        {
            daoProfilePermission = new DaoProfilePermission(context);
        }

        public IQueryable<ProfilePermission> All()
        {
            return daoProfilePermission.All();
        }

        public async Task<int> Create(ProfilePermission profilePermission)
        {
            return await daoProfilePermission.Create(profilePermission);
        }

        public async Task<ProfilePermission> Find(int? id)
        {
            return await daoProfilePermission.Find(id);
        }

        public async Task<int> Update(ProfilePermission profilePermission)
        {
            return await daoProfilePermission.Update(profilePermission);
        }

        public async Task<int> Delete(int id)
        {
            return await daoProfilePermission.Delete(id);
        }

        public bool RelationshipExists(int? securityProfileId, int? singlePermissionId)
        {
            return daoProfilePermission.RelationshipExists(securityProfileId, singlePermissionId);
        }

        public bool RelationshipExistsUpdate(int? id, int? securityProfileId, int? singlePermissionId)
        {
            return daoProfilePermission.RelationshipExistsUpdate(id, securityProfileId, singlePermissionId);
        }
    }
}
