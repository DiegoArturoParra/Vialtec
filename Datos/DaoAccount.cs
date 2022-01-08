using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoAccount
    {
        private readonly VialtecContext _context;

        public DaoAccount(VialtecContext context)
        {
            _context = context;
        }

        public async Task<CustomerUser> Login(string email, string passKey)
        {
            // Obtener el customerUser con los parámetros de autenticación
            var customerUser = await _context.CustomerUsers.Include(x => x.CustomerInfo)
                                    .Where(x => x.Email.ToLower() == email.ToLower() && x.PassKey == passKey)
                                    .FirstOrDefaultAsync();
            if (customerUser != null)
            {
                customerUser.CustomerInfo.LogoBase64 = null;
            }
            return customerUser;
        }

    }
}
