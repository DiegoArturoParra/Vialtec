using Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Logica
{
    public class LEquipmentGroup
    {
        private DaoEquipmentGroup daoEquipmentGroup;

        public LEquipmentGroup(VialtecContext context)
        {
            daoEquipmentGroup = new DaoEquipmentGroup(context);
        }

        public IQueryable<EquipmentGroup> All()
        {
            return daoEquipmentGroup.All();
        }

        public async Task<int> Create(EquipmentGroup equipmentGroup)
        {
            return await daoEquipmentGroup.Create(equipmentGroup);
        }

        public async Task<EquipmentGroup> Find(int? id)
        {
            return await daoEquipmentGroup.Find(id);
        }

        public async Task<int> Update(EquipmentGroup equipmentGroup)
        {
            return await daoEquipmentGroup.Update(equipmentGroup);
        }

        public async Task<int> Delete(int id)
        {
            return await daoEquipmentGroup.Delete(id);
        }
    }
}
