using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoSpeedReportCustomer
    {
        private readonly VialtecContext _context;

        public DaoSpeedReportCustomer(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<SpeedReportCustomer> All()
        {
            // Obtener todos los registros SpeedReportsCustomer
            var results = from x in _context.SpeedReportsCustomer
                          select x;
            return results;
        }

        public async Task<int> Create(SpeedReportCustomer speedReportCustomer)
        {
            // Crear nuevo registro SpeedReportCustomer
            _context.SpeedReportsCustomer.Add(speedReportCustomer);
            return await _context.SaveChangesAsync();
        }

        public async Task<SpeedReportCustomer> Find(int? id)
        {
            // Buscar registro SpeedReportCustomer por id
            return await _context.SpeedReportsCustomer.FindAsync(id);
        }

        public async Task<int> Update(SpeedReportCustomer speedReportCustomer)
        {
            // Actualizar registro SpeedReportCustomer
            _context.SpeedReportsCustomer.Update(speedReportCustomer);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro SpeedReportCustomer
            var speedReportCustomer = await _context.SpeedReportsCustomer.FindAsync(id);
            _context.SpeedReportsCustomer.Remove(speedReportCustomer);
            return await _context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            // Verificar si existe el registro de SpeedReportCustomer
            return _context.SpeedReportsCustomer.Any(e => e.Id == id);
        }
    }
}
