using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoAccountAdmin
    {
        private readonly VialtecContext _context;

        public DaoAccountAdmin(VialtecContext context)
        {
            _context = context;
        }

        public async Task<DistributorUser> Login(string email, string passKey)
        {
            // Obtener el distributoUser con los parámetros de autenticación
            var distributorUser = await _context.DistributorUsers
                                    .Include(x => x.DistributorInfo)
                                    .Where(x => x.Email.ToLower() == email.ToLower() && x.PassKey == passKey)
                                    .FirstOrDefaultAsync();
            return distributorUser;
        }
    }
}
