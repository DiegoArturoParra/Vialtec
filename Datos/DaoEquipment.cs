using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos
{
    public class DaoEquipment
    {
        private readonly VialtecContext _context;

        public DaoEquipment(VialtecContext context)
        {
            _context = context;
        }

        public IQueryable<Equipment> All()
        {
            // Obtener todos los registros Equipment
            var results = from c in _context.Equipments
                          select c;
            return results;
        }

        public async Task<int> Create(Equipment equipment)
        {
            // Crear nuevo registro Equipment
            _context.Equipments.Add(equipment);
            return await _context.SaveChangesAsync();
        }

        public async Task<Equipment> Find(int? id)
        {
            // Buscar registro Equipment por id
            return await _context.Equipments.FindAsync(id);
        }

        public async Task<int> Update(Equipment equipment)
        {
            // Actualizar registro Equipment
            _context.Update(equipment);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            // Eliminar registro Equipment
            var equipment = await _context.Equipments.FindAsync(id);
            _context.Equipments.Remove(equipment);
            return await _context.SaveChangesAsync();
        }
    }
}
