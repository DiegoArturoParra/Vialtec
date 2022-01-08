using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCoefficientFriction
    {
        private readonly VialtecContext _context;

        public DaoCoefficientFriction(VialtecContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retornar el IQueryable de todos los registros
        /// </summary>
        /// <returns></returns>
        public IQueryable<CoefficientFriction> All()
        {
            var results = from x in _context.CoefficientFrictions
                          select x;
            return results;
        }

        /// <summary>
        /// Agregar un listado de registros al mismo tiempo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> AddRange(IEnumerable<CoefficientFriction> data)
        {
            _context.CoefficientFrictions.AddRange(data);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
