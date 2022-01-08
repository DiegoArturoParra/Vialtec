using Datos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LReflectivity
    {
        private readonly DaoReflectivity _daoReflectivity;

        public LReflectivity(VialtecContext context)
        {
            _daoReflectivity = new DaoReflectivity(context);
        }

        public async Task<bool> AddRange(IEnumerable<Reflectivity> data)
        {
            return await _daoReflectivity.AddRange(data);
        }
    }
}
