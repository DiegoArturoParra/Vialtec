using System.Collections.Generic;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoReflectivity
    {
        private readonly VialtecContext _context;

        public DaoReflectivity(VialtecContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRange(IEnumerable<Reflectivity> data)
        {
            _context.Reflectivities.AddRange(data);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
