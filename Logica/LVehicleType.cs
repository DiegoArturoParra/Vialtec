using Datos;
using System.Linq;
using Utilitarios;

namespace Logica
{
    public class LVehicleType
    {
        private readonly DaoVehicleType _daoVehicleType;

        public LVehicleType(VialtecContext context)
        {
            _daoVehicleType = new DaoVehicleType(context);
        }

        public IQueryable<VehicleType> All()
        {
            return _daoVehicleType.All();
        }
    }
}
