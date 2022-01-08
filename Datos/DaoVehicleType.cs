using System.Linq;
using Utilitarios;

namespace Datos
{
    public class DaoVehicleType
    {
        private readonly VialtecContext _context;

        public DaoVehicleType(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<VehicleType> All()
        {
            var results = from x in _context.VehicleTypes
                          select x;
            return results;
        }
    }
}
