using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LStationarySpeedRadar
    {
        private readonly DaoStationarySpeedRadar daoStationarySpeedRadar;

        public LStationarySpeedRadar(VialtecContext context)
        {
            daoStationarySpeedRadar = new DaoStationarySpeedRadar(context);
        }

        public IQueryable<StationarySpeedRadar> All()
        {
            return daoStationarySpeedRadar.All();
        }

        public IEnumerable<StationarySpeedRadar> All(FilterStationarySpeedRadar filter)
        {
            return daoStationarySpeedRadar.All(filter);
        }

        public async Task<List<StationarySpeedRadarItemHour>> GetDataStationarySpeedRadarByHours(FilterStationarySpeedRadar filter)
        {
            return await daoStationarySpeedRadar.GetDataStationarySpeedRadarByHours(filter);
        }

        public async Task<bool> AddRange(IEnumerable<StationarySpeedRadar> data)
        {
            return await daoStationarySpeedRadar.AddRange(data);
        }
    }
}
