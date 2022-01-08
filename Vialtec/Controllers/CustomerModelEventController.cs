using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilitarios;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class CustomerModelEventController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LCustomerModelEvent logicCustomerModelEvent;

        public CustomerModelEventController(VialtecContext context)
        {
            _context = context;
            logicCustomerModelEvent = new LCustomerModelEvent(context);
        }

        public async Task<IActionResult> Index(int? pageNumber, int? modelId)
        {
            //if (!await AccessGranted())
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}

            // Obtener el customerInfoId de los Claims definidos en la autentificación
            int customerInfoId = GetCustomerInfoId();

            // Número de registros por página
            int pageSize = 40;
            // Será usado para calcular el número de páginas
            int totalPages = 0;

            // ViewDatas filtros
            ViewData["models"] = _context.Models.ToList();
            ViewData["modelId"] = modelId;

            // Consulta de registros EquipmentGroup por customerInfoId
            var query = logicCustomerModelEvent.All().Include(x => x.ModelEvent).ThenInclude(x => x.Model)
                        .Where(x => x.CustomerInfoId == GetCustomerInfoId());

            // Filtro Model
            if (modelId != null && modelId != -1)
            {
                query = query.Where(x => x.ModelEvent.ModelId == modelId);
            }

            // Si no hay registros
            if (query.ToList().Count() == 0)
            {
                ViewData["emptyMessage"] = "No se encontraron resultados";
            }

            // Calcular el número de páginas
            decimal result = decimal.Divide(query.ToList().Count(), pageSize);
            // Convertir por ejemplo:  19.3 a 20 páginas
            totalPages = (result - (int)result) != 0 ? (int)result + 1 : (int)result;
            ViewData["totalPages"] = totalPages;

            // Pasar el query de EquipmentGroup por el modelo de paginación
            return View(await PaginatedList<CustomerModelEvent>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Id), pageNumber ?? 1, pageSize));
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        public JsonResult GetCustomerModelEventsByModelId(int modelId)
        {
            var results = _context.CustomerModelEvents.Include(x => x.ModelEvent).ThenInclude(x => x.Model)
                            .Where(x => x.ModelEvent.ModelId == modelId);
            results = results.Include(x => x.ModelEvent).ThenInclude(x => x.Event);
            return Json(results);
        }
    }
}