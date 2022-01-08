using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoEquipmentGroup
    {
        private readonly VialtecContext _context;

        public DaoEquipmentGroup(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<EquipmentGroup> All()
        {
            // Obtener todos los registros EquipmentGroup
            var results = from c in _context.EquipmentGroups
                          select c;
            return results;
        }

        public async Task<int> Create(EquipmentGroup equipmentGroup)
        {
            // Crear nuevo registro equipmentGroup
            _context.EquipmentGroups.Add(equipmentGroup);
            return await _context.SaveChangesAsync();
        }

        public async Task<EquipmentGroup> Find(int? id)
        {
            // Buscar registro EquipmentGroup por id
            return await _context.EquipmentGroups.FindAsync(id);
        }

        public async Task<int> Update(EquipmentGroup equipmentGroup)
        {
            // Actualizar registro EquipmentGroup
            _context.Update(equipmentGroup);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro EquipmentGroup
            var equipmentGroup = await _context.EquipmentGroups.FindAsync(id);
            _context.EquipmentGroups.Remove(equipmentGroup);
            return await _context.SaveChangesAsync();
        }
    }
}
