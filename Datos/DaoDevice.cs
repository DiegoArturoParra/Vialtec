using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoDevice
    {
        private readonly VialtecContext _context;

        public DaoDevice(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Device> All()
        {
            // Obtener todos los registros Devices
            var results = from x in _context.Devices
                          select x;
            return results;
        }

        public async Task<int> Create(Device device)
        {
            // Crear nuevo registro device
            _context.Devices.Add(device);
            return await _context.SaveChangesAsync();
        }

        public async Task<Device> Find(int? id)
        {
            // Buscar registro device por id
            return await _context.Devices.FindAsync(id);
        }

        public async Task<int> Update(Device device)
        {
            // Actualizar registro device
            _context.Devices.Update(device);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro device
            var device = await _context.Devices.FindAsync(id);
            _context.Devices.Remove(device);
            return await _context.SaveChangesAsync();
        }
    }
}
