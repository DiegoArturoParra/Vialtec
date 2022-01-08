using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LSubproject
    {
        private DaoSubproject daoSubproject;

        public LSubproject(VialtecContext context)
        {
            daoSubproject = new DaoSubproject(context);
        }

        public IQueryable<Subproject> All()
        {
            return daoSubproject.All();
        }

        public async Task<int> Create(Subproject subproject)
        {
            return await daoSubproject.Create(subproject);
        }

        public async Task<Subproject> Find(int? id)
        {
            return await daoSubproject.Find(id);
        }

        public async Task<int> Update(Subproject subproject)
        {
            return await daoSubproject.Update(subproject);
        }

        public async Task<int> Delete(int id)
        {
            return await daoSubproject.Delete(id);
        }
    }
}
