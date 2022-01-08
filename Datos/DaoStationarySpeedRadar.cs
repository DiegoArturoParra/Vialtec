using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoStationarySpeedRadar
    {
        private readonly VialtecContext _context;

        public DaoStationarySpeedRadar(VialtecContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Se encarga de retornar el IQueryable total de los registros de la tabla
        /// </summary>
        /// <returns></returns>
        public IQueryable<StationarySpeedRadar> All()
        {
            // Obtener todos los registros Stationary Speed Radar
            var results = from c in _context.StationarySpeedRadars
                          select c;
            return results;
        }

        /// <summary>
        /// Se encarga de realizar una consulta al procedimiento almacenado que utiliza cursores
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <param name="subprojectId"></param>
        /// <returns></returns>
        public IEnumerable<StationarySpeedRadar> All(FilterStationarySpeedRadar filter)
        {
            var query = _context.StationarySpeedRadars.Where(x => false);
            if (filter.EquipmentId != null)
            {
                // Filtrar por un dispositivo en especifico
                query = from x in _context.StationarySpeedRadars.Include(x => x.VehicleType)
                        let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == filter.SpeedReportId && z.VehicleTypeId == x.VehicleTypeId)
                        where x.EquipmentId == filter.EquipmentId
                        && (filter.SubprojectId == null || x.SubprojectId == filter.SubprojectId)
                        && x.DeviceDt >= filter.DateInit && x.DeviceDt <= filter.DateFinal
                        orderby x.DeviceDt descending
                        select new StationarySpeedRadar
                        {
                            DeviceDt = x.DeviceDt,
                            ServerDt = x.ServerDt,
                            Speed = x.Speed,
                            EquipmentId = x.EquipmentId,
                            VehicleTypeId = x.VehicleTypeId,
                            VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                        };
            }
            else
            {
                // Obtener IDs Dispositivos del cliente actual
                var equipmentsIDs = _context.Equipments.Include(x => x.EquipmentGroup)
                                       .Where(x => x.EquipmentGroup.CustomerInfoId == filter.CustomerInfoId)
                                       .Select(x => x.Id).ToList();
                // Filtrar por los dispositivos del cliente actual
                query = from x in _context.StationarySpeedRadars.Include(x => x.VehicleType)
                        let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == filter.SpeedReportId && z.VehicleTypeId == x.VehicleTypeId)
                        where equipmentsIDs.Contains(x.EquipmentId)
                        && (filter.SubprojectId == null || x.SubprojectId == filter.SubprojectId)
                        && x.DeviceDt >= filter.DateInit && x.DeviceDt <= filter.DateFinal
                        orderby x.DeviceDt descending
                        select new StationarySpeedRadar
                        {
                            DeviceDt = x.DeviceDt,
                            ServerDt = x.ServerDt,
                            Speed = x.Speed,
                            EquipmentId = x.EquipmentId,
                            VehicleTypeId = x.VehicleTypeId,
                            VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                        };
            }
            return query.ToList();
        }

        /// <summary>
        /// Se encarga de devolver el reporte de horas con el filtro de fecha y dispositivo
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <returns></returns>
        public async Task<List<StationarySpeedRadarItemHour>> GetDataStationarySpeedRadarByHours(FilterStationarySpeedRadar filter)
        {
            var results = new List<StationarySpeedRadarItemHour>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                #region "Preparar comando"
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "reports.f_stationary_speed_radar_by_hour"; // function name
                                                                                  // PARAMETERS
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_date_init", NpgsqlDbType.Timestamp) { Value = filter.DateInit });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_date_final", NpgsqlDbType.Timestamp) { Value = filter.DateFinal });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("p_customer_info_id", NpgsqlDbType.Integer) { Value = filter.CustomerInfoId });
                // Opcionales
                if (filter.EquipmentId != null)
                {
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("p_equipment_id", NpgsqlDbType.Integer) { Value = filter.EquipmentId });
                }
                if (filter.SubprojectId != null)
                {
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("p_subproject_id", NpgsqlDbType.Integer) { Value = filter.SubprojectId });
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
                        var item = new StationarySpeedRadarItemHour
                        {
                            Hour = (i > 9 ? $"{i}" : $"0{i}") + ":00",
                            TotalResults = data[0, i],
                            AverageSpeed = data[1, i]
                        };
                        results.Add(item);
                    }
                    #endregion
                }
            }
            return results;
        }

        /// <summary>
        /// Agregar un listado de registros de Stationary Speed Radar
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> AddRange(IEnumerable<StationarySpeedRadar> data)
        {
            _context.AddRange(data);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
