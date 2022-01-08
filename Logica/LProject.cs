using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LProject
    {
        private DaoProject daoProject;

        public LProject(VialtecContext context)
        {
            daoProject = new DaoProject(context);
        }

        public IQueryable<Project> All()
        {
            return daoProject.All();
        }

        public async Task<int> Create(Project project)
        {
            return await daoProject.Create(project);
        }

        public async Task<Project> Find(int? id)
        {
            return await daoProject.Find(id);
        }

        public async Task<int> Update(Project project)
        {
            return await daoProject.Update(project);
        }

        public async Task<int> Delete(int id)
        {
            return await daoProject.Delete(id);
        }
    }
}
