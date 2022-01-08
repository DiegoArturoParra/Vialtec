using Datos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LSpeedStatReport
    {
        private readonly DaoSpeedStatReport daoSpeedStatReport;

        public LSpeedStatReport(VialtecContext context)
        {
            daoSpeedStatReport = new DaoSpeedStatReport(context);
        }

        public IQueryable<SpeedStatReport> All()
        {
            // Obtener todos los registros SpeedStatReports
            return daoSpeedStatReport.All();
        }

        public async Task<int> Create(SpeedStatReport model)
        {
            // Crear nuevo registro SpeedStatReport
            return await daoSpeedStatReport.Create(model);
        }

        public async Task<SpeedStatReport> Find(int? id)
        {
            // Buscar registro SpeedStatReport por id
            return await daoSpeedStatReport.Find(id);
        }

        public async Task<int> Update(SpeedStatReport model)
        {
            // Actualizar registro SpeedStatReport
            return await daoSpeedStatReport.Update(model);
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro SpeedStatReport
            return await daoSpeedStatReport.Delete(id);
        }

        public async Task<List<SpeedStatReportItemHour>> GetDataStationarySpeedRadarByHours(FilterSpeedStatReport filter)
        {
            return await daoSpeedStatReport.GetDataStationarySpeedRadarByHours(filter);
        }
    }
}
