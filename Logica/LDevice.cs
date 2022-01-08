using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LDevice
    {
        private DaoDevice daoDevice;

        public LDevice(VialtecContext context)
        {
            daoDevice = new DaoDevice(context);
        }

        public IQueryable<Device> All()
        {
            return daoDevice.All();
        }

        public async Task<int> Create(Device device)
        {
            return await daoDevice.Create(device);
        }

        public async Task<Device> Find(int? id)
        {
            return await daoDevice.Find(id);
        }

        public async Task<int> Update(Device device)
        {
            return await daoDevice.Update(device);
        }

        public async Task<int> Delete(int id)
        {
            return await daoDevice.Delete(id);
        }
    }
}
