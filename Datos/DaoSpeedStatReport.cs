using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoSpeedStatReport
    {
        private readonly VialtecContext _context;

        public DaoSpeedStatReport(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<SpeedStatReport> All()
        {
            // Obtener todos los registros SpeedStatReports
            return _context.SpeedStatReports;
        }

        public async Task<int> Create(SpeedStatReport model)
        {
            // Crear nuevo registro SpeedStatReport
            _context.SpeedStatReports.Add(model);
            return await _context.SaveChangesAsync();
        }

        public async Task<SpeedStatReport> Find(int? id)
        {
            // Buscar registro SpeedStatReport por id
            return await _context.SpeedStatReports.FindAsync(id);
        }

        public async Task<int> Update(SpeedStatReport model)
        {
            // Actualizar registro SpeedStatReport
            _context.SpeedStatReports.Update(model);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar un registro SpeedStatReport
            var model = await _context.SpeedStatReports.FindAsync(id);
            _context.SpeedStatReports.Remove(model);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Se encarga de devolver el reporte de horas con el filtro de fecha y dispositivo
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <returns></returns>
        public async Task<List<SpeedStatReportItemHour>> GetDataStationarySpeedRadarByHours(FilterSpeedStatReport filter)
        {
            var results = new List<SpeedStatReportItemHour>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                #region "Preparar comando"
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "reports.f_speed_stat_report_by_hour"; // function name
                                                                                  // PARAMETERS
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_date_init", NpgsqlDbType.Timestamp) { Value = filter.DateInit });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_date_final", NpgsqlDbType.Timestamp) { Value = filter.DateFinal });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_customer_info_id", NpgsqlDbType.Integer) { Value = filter.CustomerInfoId });
                // Opcionales
                if (filter.EquipmentId != null)
                {
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("p_equipment_id", NpgsqlDbType.Integer) { Value = filter.EquipmentId });
                }
                #endregion

                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();

                var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows && reader.Read())
                {
                    #region "Crear listado"
                    // Matriz {total results, averages}
                    var data = (decimal[,])reader.GetValue(0);
                    // Recorrer las horas del día desde 00:00 a 23:00
                    for (int i = 0; i <= 23; i++)
                    {
                        var item = new SpeedStatReportItemHour
                        {
                            Hour = (i > 9 ? $"{i}" : $"0{i}") + ":00",
                            TotalResults = data[0, i],
                            AverageAvgSpeed = data[1, i],
                            AverageLastSpeed = data[2, i],
                            AveragePeakSpeed = data[3, i]
                        };
                        results.Add(item);
                    }
                    #endregion
                }
            }
            return results;
        }
    }
}
