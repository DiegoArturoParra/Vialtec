using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilitarios;

namespace Datos
{
    public class DaoAccountSuperAdmin
    {
        private readonly VialtecContext _context;

        public DaoAccountSuperAdmin(VialtecContext context)
        {
            _context = context;
        }

        public SAdmin Login(string email, string passKey)
        {
            // Obtener el superAdmin con los parámetros de autenticación
            var superAdmin = _context.SuperAdmins
                            .Where(x => x.Email.ToLower().Equals(email.ToLower()) && x.Password.Equals(passKey))
                            .FirstOrDefault();
            return superAdmin;
        }
    }
}
