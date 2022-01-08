using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LPrecommandByUser
    {
        private readonly DaoPrecommandByUser daoPrecommandByUser;

        public LPrecommandByUser(VialtecContext context)
        {
            daoPrecommandByUser = new DaoPrecommandByUser(context);
        }

        public IQueryable<PrecommandByUser> All()
        {
            return daoPrecommandByUser.All();
        }

        public async Task<int> CreateRange(List<PrecommandByUser> precommandsByUser)
        {
            return await daoPrecommandByUser.CreateRange(precommandsByUser);
        }

        public async Task<PrecommandByUser> Find(int? id)
        {
            return await daoPrecommandByUser.Find(id);
        }

        public async Task<int> Update(PrecommandByUser precommandByUser)
        {
            return await daoPrecommandByUser.Update(precommandByUser);
        }

        public async Task<int> DeleteRange(List<PrecommandByUser> precommandsByUser)
        {
            return await daoPrecommandByUser.DeleteRange(precommandsByUser);
        }
    }
}
