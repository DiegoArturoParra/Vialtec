using Datos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCoefficientFriction
    {
        private readonly DaoCoefficientFriction daoCoefficientFriction;

        public LCoefficientFriction(VialtecContext context)
        {
            daoCoefficientFriction = new DaoCoefficientFriction(context);
        }

        public IQueryable<CoefficientFriction> All()
        {
            return daoCoefficientFriction.All();
        }

        public async Task<bool> AddRange(IEnumerable<CoefficientFriction> data)
        {
            return await daoCoefficientFriction.AddRange(data);
        }
    }
}
