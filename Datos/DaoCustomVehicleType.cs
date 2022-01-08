using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoCustomVehicleType
    {
        private readonly VialtecContext _context;

        public DaoCustomVehicleType(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<CustomVehicleType> All()
        {
            // Obtener todos los registros CustomVehicleTypes
            var results = from x in _context.CustomVehicleTypes
                          select x;
            return results;
        }

        public async Task<int> Create(CustomVehicleType customVehicleType)
        {
            // Crear nuevo registro CustomVehicleType
            _context.CustomVehicleTypes.Add(customVehicleType);
            return await _context.SaveChangesAsync();
        }

        public async Task<CustomVehicleType> Find(int? id)
        {
            // Buscar registro CustomVehicleType por id
            return await _context.CustomVehicleTypes.FindAsync(id);
        }

        public async Task<int> Update(CustomVehicleType customVehicleType)
        {
            // Actualizar registro CustomVehicleType
            _context.CustomVehicleTypes.Update(customVehicleType);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro CustomVehicleType
            var customVehicleType = await _context.CustomVehicleTypes.FindAsync(id);
            _context.CustomVehicleTypes.Remove(customVehicleType);
            return await _context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            // Determinar si un registro existe
            return _context.CustomVehicleTypes.Any(x => x.Id == id);
        }
    }
}
