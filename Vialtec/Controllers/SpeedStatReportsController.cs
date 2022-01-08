using System;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilitarios;
using Vialtec.Models;
using ClosedXML.Excel;
using System.IO;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class SpeedStatReportsController : Controller
    {
        private readonly LEquipment _logicEquipment;
        private readonly LVehicleType _logicVehicleType;
        private readonly LSpeedStatReport _logicSpeedStatReport;

        public SpeedStatReportsController(VialtecContext context)
        {
            _logicEquipment = new LEquipment(context);
            _logicVehicleType = new LVehicleType(context);
            _logicSpeedStatReport = new LSpeedStatReport(context);
        }

        public async Task<IActionResult> Index(int? pageNumber, int? equipmentId, string dateInit, string dateFinal)
        {
            // Número de registros por página
            int pageSize = 50;

            // Fecha actual
            var today = DateTime.Today;

            // ViewDatas
            var equipments = _logicEquipment.All().Include(x => x.EquipmentGroup)
                             .Where(x => x.EquipmentGroup.CustomerInfoId == GetCustomerInfoId());

            ViewData["equipmentId"] = equipmentId;
            ViewData["dateInit"] = !string.IsNullOrEmpty(dateInit) ? dateInit : today.ToString("yyyy-MM-dd");
            ViewData["dateFinal"] = !string.IsNullOrEmpty(dateFinal) ? dateFinal : today.ToString("yyyy-MM-dd");
            ViewData["equipments"] = equipments.ToList();
            ViewData["vehicleTypes"] = _logicVehicleType.All().OrderBy(x => x.Id).ToList();

            // ID dispositivos del cliente
            var equipmentIds = equipments.Select(x => x.Id).ToList();

            // Consulta
            var query = _logicSpeedStatReport.All().Where(x => equipmentIds.Contains(x.EquipmentId));

            // Aplicar filtros
            if (equipmentId != null)
            {
                query = query.Where(x => x.EquipmentId == equipmentId);
            }

            if (!string.IsNullOrEmpty(dateInit) && !string.IsNullOrEmpty(dateFinal))
            {
                var fechaInicial = Convert.ToDateTime(dateInit);
                var fechaFinal = Convert.ToDateTime(dateFinal).AddDays(1).AddMinutes(-1);
                query = query.Where(x => x.DeviceDt >= fechaInicial && x.DeviceDt <= fechaFinal);
            } else
            {
                query = query.Where(x => x.DeviceDt >= today && x.DeviceDt <= today.AddDays(1).AddMinutes(-1));
            }

            // Includes
            query = query.Include(x => x.Equipment).Include(x => x.VehicleType);

            // Total pages
            ViewData["totalPages"] = (int)Math.Ceiling(query.Count() / (double)pageSize);

            return View(await PaginatedList<SpeedStatReport>.CreateAsync(query.AsNoTracking().OrderByDescending(x => x.DeviceDt), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Se encarga de retornar toda la data necesaria para mostrar la gráfica de velocidades por hora
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetDataSpeedStatReportByHours(int? equipmentId, string dateInit, string dateFinal)
        {
            try
            {
                var dtInit = Convert.ToDateTime(dateInit);
                var dtFinal = Convert.ToDateTime(dateFinal).AddDays(1).AddMinutes(-1);
                var filter = new FilterSpeedStatReport
                {
                    EquipmentId = equipmentId,
                    CustomerInfoId = GetCustomerInfoId(),
                    DateInit = dtInit,
                    DateFinal = dtFinal
                };
                var results = await _logicSpeedStatReport.GetDataStationarySpeedRadarByHours(filter);
                return Json(results);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Obtener el ID del cliente
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Descargar archivo excel
        /// </summary>
        /// <param name="muReportId_xls"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportExcel(int? equipmentId_xls, string dateInit_xls, string dateFinal_xls)
        {
            var equipments = _logicEquipment.All().Include(x => x.EquipmentGroup)
                 .Where(x => x.EquipmentGroup.CustomerInfoId == GetCustomerInfoId());
            var equipmentIds = equipments.Select(x => x.Id).ToList();

            var query = _logicSpeedStatReport.All().Where(x => equipmentIds.Contains(x.EquipmentId));

            if (equipmentId_xls != null)
            {
                query = query.Where(x => x.EquipmentId == equipmentId_xls);
            }

            if (!string.IsNullOrEmpty(dateInit_xls) && !string.IsNullOrEmpty(dateFinal_xls))
            {
                var fechaInicial = Convert.ToDateTime(dateInit_xls);
                var fechaFinal = Convert.ToDateTime(dateFinal_xls).AddDays(1).AddMinutes(-1);
                query = query.Where(x => x.DeviceDt >= fechaInicial && x.DeviceDt <= fechaFinal);
            }
            else
            {
                query = query.Where(x => x.DeviceDt >= DateTime.Today && x.DeviceDt <= DateTime.Today.AddDays(1).AddMinutes(-1));
            }

            query = query.Include(x => x.Equipment).Include(x => x.VehicleType).OrderByDescending(x => x.DeviceDt);

            // Generando Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte");
                var currentRow = 1;
                // Cabeceros
                worksheet.Cell(currentRow, 1).Value = "Fecha Dispositivo";
                worksheet.Cell(currentRow, 2).Value = "Fecha Servidor";
                worksheet.Cell(currentRow, 3).Value = "Dispositivo";
                worksheet.Cell(currentRow, 4).Value = "Tipo Vehículo";
                worksheet.Cell(currentRow, 5).Value = "Pico Velocidad";
                worksheet.Cell(currentRow, 6).Value = "Última Velocidad";
                worksheet.Cell(currentRow, 7).Value = "Promedio Velocidad";
                // Data
                foreach (var item in query)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.DeviceDt;
                    worksheet.Cell(currentRow, 2).Value = item.ServerDt;
                    worksheet.Cell(currentRow, 3).Value = item.Equipment.EquipmentAlias;
                    worksheet.Cell(currentRow, 4).Value = item.VehicleType.Title;
                    worksheet.Cell(currentRow, 5).Value = item.PeakSpeed + " Km/h";
                    worksheet.Cell(currentRow, 6).Value = item.LastSpeed + " Km/h";
                    worksheet.Cell(currentRow, 7).Value = item.AverageSpeed + " Km/h";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string nameFile = $"Radar_con_estadística_{dateString}.xlsx";
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nameFile);
                }
            }

        }
    }
}