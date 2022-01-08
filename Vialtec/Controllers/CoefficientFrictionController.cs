using ClosedXML.Excel;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using Utilitarios;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class CoefficientFrictionController : Controller
    {
        private readonly VialtecContext _context;
        private readonly LSubproject _logicSubprect;
        private readonly LCoefficientFriction _logicCoefficientFriction;

        public CoefficientFrictionController(VialtecContext context)
        {
            _context = context;
            _logicSubprect = new LSubproject(context);
            _logicCoefficientFriction = new LCoefficientFriction(context);
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="date"></param>
        /// <param name="subprojectId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? pageNumber, int? muReportId, int? minimo, int? tolerancia)
        {
            // Número de registros por página
            int pageSize = 100;

            // ViewDatas
            ViewData["minimo"] = minimo;
            ViewData["tolerancia"] = tolerancia;
            ViewData["muReportId"] = muReportId;
            ViewData["muReports"] = _context.MuReports.Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToList();

            // Consulta
            var query = _logicCoefficientFriction.All().Include(x => x.MuReport)
                            .Where(x => x.MuReport.CustomerInfoId == GetCustomerInfoId());

            // APLICAR FILTROS

            if (muReportId != null)
            {
                query = query.Where(x => x.MuReportId == muReportId);
            }

            // Total pages
            ViewData["totalPages"] = (int)Math.Ceiling(query.Count() / (double)pageSize);

            // Pasamos el query de Equipments por el modelo de paginación
            return View(await PaginatedList<CoefficientFriction>.CreateAsync(query.AsNoTracking().OrderBy(x => x.Odometer), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Obtener los registros filtrados
        /// </summary>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetDataByFilters(int? muReportId)
        {
            var query = _logicCoefficientFriction.All();
            if (muReportId != null)
            {
                query = query.Where(x => x.MuReportId == muReportId);
            }

            return Json(await query.ToListAsync());
        }

        /// <summary>
        /// Obtener el registro CoefficientFriction por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetCoefficientFrictionById(int id)
        {
            var coefficientFriction = _logicCoefficientFriction.All().Include(x => x.MuReport)
                                        .FirstOrDefault(x => x.Id == id);
            return Json(coefficientFriction);
        }

        /// <summary>
        /// Obtener el customerInfoId del cliente actual
        /// </summary>
        /// <returns></returns>
        public int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Exportar archivo Excel
        /// </summary>
        /// <param name="muReportId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportExcel(int? muReportId_xls)
        {
            // Consulta
            var query = _logicCoefficientFriction.All().Include(x => x.MuReport)
                            .Where(x => x.MuReport.CustomerInfoId == GetCustomerInfoId());

            if (muReportId_xls != null)
            {
                query = query.Where(x => x.MuReportId == muReportId_xls);
            }
            var results = await query.OrderBy(x => x.Odometer).Select(x => new CoefficientFriction {
                                                Latitude = x.Latitude,
                                                Longitude = x.Longitude,
                                                Mu = x.Mu,
                                                Odometer = x.Odometer
                                            }).ToListAsync();

            // Generando Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte");
                var currentRow = 1;
                // Cabeceros
                worksheet.Cell(currentRow, 1).Value = "Latitud";
                worksheet.Cell(currentRow, 2).Value = "Longitud";
                worksheet.Cell(currentRow, 3).Value = "Mu";
                worksheet.Cell(currentRow, 4).Value = "Odómetro";
                // Data
                foreach (var item in results)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Latitude;
                    worksheet.Cell(currentRow, 2).Value = item.Longitude;
                    worksheet.Cell(currentRow, 3).Value = item.Mu;
                    worksheet.Cell(currentRow, 4).Value = item.Odometer;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string nameFile = $"Coeficiente_Fricción_{dateString}.xlsx";
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nameFile);
                }
            }

        }


        /// <summary>
        /// Método encargado de retornar el archivo KML con los resultados
        /// </summary>
        /// <param name="muReportId_xls"></param>
        [HttpPost]
        public async Task ExportKml(int? muReportId_xls)
        {
            // Consulta
            var query = _logicCoefficientFriction.All().Include(x => x.MuReport)
                            .Where(x => x.MuReport.CustomerInfoId == GetCustomerInfoId());

            if (muReportId_xls != null)
            {
                query = query.Where(x => x.MuReportId == muReportId_xls);
            }
            var results = await query.OrderBy(x => x.Odometer).Select(x => new CoefficientFriction
                                {
                                    Latitude = x.Latitude,
                                    Longitude = x.Longitude,
                                    Mu = x.Mu
                                }).ToListAsync();

            string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nameFileKml = $"Coeficiente_Fricción_{dateString}.kml";
            Response.ContentType = "application/vnd.google-earth.kml+xml";
            Response.Headers.Add("Content-Disposition", $"attachment; filename={nameFileKml}");
            XmlTextWriter kml = new XmlTextWriter(HttpContext.Response.Body, System.Text.Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 3
            };
            kml.WriteStartDocument();

            kml.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");

            kml.WriteStartElement("Document");
            kml.WriteElementString("name", "Coeficiente de Fricción");

            kml.WriteStartElement("Folder");
            kml.WriteElementString("name", "Medidas");
            kml.WriteElementString("open", "1");

            // Recorrer el resultado del filtrado
            foreach (var item in results)
            {
                kml.WriteStartElement("Placemark");
                kml.WriteElementString("name", item.Mu.ToString());
                kml.WriteElementString("styleUrl", "#m_ylw-pushpin");

                kml.WriteStartElement("Point");
                kml.WriteElementString("coordinates", $"{item.Longitude.ToString().Replace(',', '.')},{item.Latitude.ToString().Replace(',', '.')}");

                kml.WriteEndElement(); // <Point>
                kml.WriteEndElement(); // <Placemark>
            }

            kml.WriteEndElement(); // <Folder>
            kml.WriteEndElement(); // <Document>

            kml.WriteEndDocument(); // <kml>

            kml.Close();
        }
    }
}