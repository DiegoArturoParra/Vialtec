using Datos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LCustomVehicleType
    {
        private readonly DaoCustomVehicleType _daoCustomVehicleType;

        public LCustomVehicleType(VialtecContext context)
        {
            _daoCustomVehicleType = new DaoCustomVehicleType(context);
        }

        public IQueryable<CustomVehicleType> All()
        {
            // Obtener todos los registros CustomVehicleTypes
            return _daoCustomVehicleType.All();
        }

        public async Task<int> Create(CustomVehicleType customVehicleType)
        {
            // Crear nuevo registro CustomVehicleType
            return await _daoCustomVehicleType.Create(customVehicleType);
        }

        public async Task<CustomVehicleType> Find(int? id)
        {
            // Buscar registro CustomVehicleType por id
            return await _daoCustomVehicleType.Find(id);
        }

        public async Task<int> Update(CustomVehicleType customVehicleType)
        {
            // Actualizar registro CustomVehicleType
            return await _daoCustomVehicleType.Update(customVehicleType);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomVehicleType
            return await _daoCustomVehicleType.Delete(id);
        }

        public bool Exists(int id)
        {
            // Determinar si un registro existe
            return _daoCustomVehicleType.Exists(id);
        }
    }
}
