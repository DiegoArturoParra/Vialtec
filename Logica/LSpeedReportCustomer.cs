using Datos;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LSpeedReportCustomer
    {
        private readonly DaoSpeedReportCustomer _daoSpeedReportCustomer;

        public LSpeedReportCustomer(VialtecContext context)
        {
            _daoSpeedReportCustomer = new DaoSpeedReportCustomer(context);
        }

        public IQueryable<SpeedReportCustomer> All()
        {
            return _daoSpeedReportCustomer.All();
        }

        public async Task<int> Create(SpeedReportCustomer speedReportCustomer)
        {
            return await _daoSpeedReportCustomer.Create(speedReportCustomer);
        }

        public async Task<SpeedReportCustomer> Find(int? id)
        {
            return await _daoSpeedReportCustomer.Find(id);
        }

        public async Task<int> Update(SpeedReportCustomer speedReportCustomer)
        {
            return await _daoSpeedReportCustomer.Update(speedReportCustomer);
        }

        public async Task<int> Delete(int id)
        {
            return await _daoSpeedReportCustomer.Delete(id);
        }

        public bool Exists(int id)
        {
            return _daoSpeedReportCustomer.Exists(id);
        }
    }
}
