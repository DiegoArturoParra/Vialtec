using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LEquipment
    {
        private DaoEquipment daoEquipment;

        public LEquipment(VialtecContext context)
        {
            daoEquipment = new DaoEquipment(context);
        }

        public IQueryable<Equipment> All()
        {
            return daoEquipment.All();
        }

        public async Task<int> Create(Equipment equipment)
        {
            return await daoEquipment.Create(equipment);
        }

        public async Task<Equipment> Find(int? id)
        {
            return await daoEquipment.Find(id);
        }

        public async Task<int> Update(Equipment equipment)
        {
            return await daoEquipment.Update(equipment);
        }

        public async Task<int> Delete(int id)
        {
            return await daoEquipment.Delete(id);
        }
    }
}
