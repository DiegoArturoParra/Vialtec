using ClosedXML.Excel;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vialtec.Helpers;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class MarkingController : Controller
    {
        private readonly VialtecContext _context;
        private static List<ReportMarking> _markings = new List<ReportMarking>();
        public MarkingController(VialtecContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!await CheckPermissions())
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Obtener el customer_info_id de los Claims del login
            int customerInfoId = GetCustomerInfoId();

            // ViewDatas para los filtros iniciales de la vista
            ViewData["datetime-inicial"] = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewData["datetime-final"] = DateTime.Today.AddDays(1).AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ss");
            ViewData["equipmentGroups"] = _context.EquipmentGroups.Where(x => x.CustomerInfoId == customerInfoId).ToList();
            return View();
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Se encarga de retornar un listado de equipments filtrado por equipmentGroupId
        /// </summary>
        /// <param name="equipmentGroupId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetEquipmentsByEquipmentGroupId(int equipmentGroupId)
        {
            // CategoryId 2 para controladores de demarcación
            //var equipments = _context.Equipments.Include(x => x.Device).ThenInclude(x => x.Model)
            //                .Where(x => x.EquipmentGroupId == equipmentGroupId && x.Device.Model.CategoryId == 2);
            var equipments = _context.Equipments
                            .Where(x => x.EquipmentGroupId == equipmentGroupId);
            return Json(equipments);
        }

        /// <summary>
        /// Se encarga de obtener los registros del reporte de traslado
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInitComplete"></param>
        /// <param name="dateFinalComplete"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTransmissionsInfo(MarkingFilter filter)
        {
            var datetimeInicial = Convert.ToDateTime(filter.DateInitComplete);
            var datetimeFinal = Convert.ToDateTime(filter.DateFinalComplete);

            var transmissionsInfo = from t in _context.TransmissionInfos
                                    where t.EquipmentId == filter.EquipmentId
                                    && t.DeviceDt >= datetimeInicial && t.DeviceDt <= datetimeFinal
                                    && t.Latitude != null && t.Longitude != null
                                    orderby t.DeviceDt
                                    select t;
            return Json(transmissionsInfo);
        }

        /// <summary>
        /// Se encarga de obtener los registros del reporte de demarcación
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInitComplete"></param>
        /// <param name="dateFinalComplete"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetMarkingResults(MarkingFilter filter)
        {
            var datetimeInicial = Convert.ToDateTime(filter.DateInitComplete);
            var datetimeFinal = Convert.ToDateTime(filter.DateFinalComplete);

            //var markings = from t in _context.Markings
            //               where t.EquipmentId == filter.EquipmentId
            //                && t.DeviceDt >= datetimeInicial && t.DeviceDt <= datetimeFinal
            //                && t.Latitude != null && t.Longitude != null
            //               orderby t.DeviceDt
            //               select t;

            var markings = _context.Markings
                            .Where(t => t.EquipmentId == filter.EquipmentId && t.DeviceDt >= datetimeInicial && t.DeviceDt <= datetimeFinal
                            && t.Latitude != null && t.Longitude != null).OrderBy(t => t.DeviceDt).GroupBy(x => x.TrackNumber)
                            .Select(x => new SummaryMarking
                            {
                                SumLeftPaintMeters = FormatDecimal(x.Sum(s => s.LeftPaintMeters)),
                                SumCenterPaintMeters = FormatDecimal(x.Sum(s => s.CenterPaintMeters)),
                                SumRightPaintMeters = FormatDecimal(x.Sum(s => s.RightPaintMeters)),
                                SumMostRightPaintMeters =FormatDecimal(x.Sum(s => s.MostRightPaintMeters)),
                                FinalDate = FormatDate(x.Max(s => s.DeviceDt)),
                                InitialDate = FormatDate(x.Min(s => s.DeviceDt)),
                                TrackNumber = x.Key,
                            }).ToList();
            return Json(markings);
        }

        private double FormatDecimal(double value)
        {
            return Math.Round(value, 2);
        }
        private string FormatDate(DateTime value)
        {
            return value.ToString("dd/MM/yyyy hh:mm:ss tt",
                  CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Se encarga de retornar el resumen de la actual consulta de demarcación
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <param name="dateInitComplete"></param>
        /// <param name="dateFinalComplete"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetSummaryQuery(MarkingFilter filter)
        {
            var dateInit = Convert.ToDateTime(filter.DateInitComplete);
            var dateFinal = Convert.ToDateTime(filter.DateFinalComplete);

            var summary = new SummaryMarking();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                #region "Prepare command"
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "reports.f_get_summary_markings"; // function name
                                                                        // Parameters
                command.Parameters.Add(new Npgsql.NpgsqlParameter("equipment_id", NpgsqlDbType.Integer) { Value = filter.EquipmentId });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("date_init", NpgsqlDbType.Timestamp) { Value = dateInit });
                command.Parameters.Add(new Npgsql.NpgsqlParameter("date_final", NpgsqlDbType.Timestamp) { Value = dateFinal });

                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();
                #endregion
                var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    #region "Management reader"
                    if (reader.HasRows && reader.Read())
                    {
                        var results = (decimal[])reader.GetValue(0);
                        // Summary object
                        summary = new SummaryMarking
                        {
                            // Metros aplicados
                            SumLeftPaintMeters = Convert.ToDouble(results[0]),
                            SumCenterPaintMeters = Convert.ToDouble(results[1]),
                            SumRightPaintMeters = Convert.ToDouble(results[2]),
                            SumMostRightPaintMeters = Convert.ToDouble(results[3]),
                            AverageSpeed = Convert.ToDouble(results[4]),
                            TotalMeters = Convert.ToDouble(results[5]),
                            TotalSeconds = Convert.ToDouble(results[6])
                        };
                    }
                    #endregion
                }
            }

            return Json(summary);
        }
        #region Generar excel con datos Clase Reporte

        [HttpPost]
        public IActionResult GenerateExcel([FromBody] IEnumerable<ReportMarking> reporte)
        {
            try
            {
                if (reporte.ToList().Count > 0)
                {
                    _markings = reporte.ToList();
                }

                return Json(new { redirect = Url.Action("ExportExcel") });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult ExportExcel()
        {
            if (_markings.Count > 0)
            {
                // Generando Excel
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reporte");
                    var currentRow = 1;
                    // Cabeceros
                    worksheet.Cell(currentRow, 1).Value = "Fecha Inicial";
                    worksheet.Cell(currentRow, 2).Value = "Fecha Final";
                    worksheet.Cell(currentRow, 3).Value = "Tiempo";
                    worksheet.Cell(currentRow, 4).Value = "Total Metros";
                    worksheet.Cell(currentRow, 5).Value = "Promedio Velocidad";
                    // Data
                    foreach (var item in _markings)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = item.fechaInicial;
                        worksheet.Cell(currentRow, 2).Value = item.fechaFinal;
                        worksheet.Cell(currentRow, 3).Value = item.tiempo;
                        worksheet.Cell(currentRow, 4).Value = item.totalMetros;
                        worksheet.Cell(currentRow, 5).Value = item.promedioVelocidad;
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string nameFile = $"demarcación_{dateString}.xlsx";
                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            nameFile);
                    }
                }
            }
            return NoContent();
        }
        #endregion


        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckPermissions()
        {
            // Los usuarios administradores tienen permiso a todas las vistas sin verificar permisos de acceso
            if (User.IsInRole("CustomerAdmin")) return true;
            // Obtener nombre del controlador
            string nameController = ControllerContext.ActionDescriptor.ControllerName;
            // Obtener customer user id
            int customerUserId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerUserId").Value);
            // Obtener los permisos para el customer user id
            var permissionIdentifiers = await _context.CustomerUserPermissions.Include(x => x.SinglePermission)
                        .Where(x => x.CustomerUserId == customerUserId)
                        .Select(x => x.SinglePermission.Identifier)
                        .ToListAsync();
            return permissionIdentifiers.Contains(nameController);
        }
    }
}