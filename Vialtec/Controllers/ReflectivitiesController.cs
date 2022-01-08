using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;
using Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Utilitarios;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class ReflectivitiesController : Controller
    {
        private readonly VialtecContext _context;

        public ReflectivitiesController(VialtecContext context)
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

            // ViewDatas de listas para los filtros
            ViewData["projects"] = _context.Projects
                                            .Where(x => x.CustomerInfoId.Equals(customerInfoId))
                                            .ToList();
            ViewData["lineColors"] = _context.LineColors.OrderBy(x => x.Id).ToList();
            ViewData["geometries"] = _context.Geometries.OrderBy(x => x.Id).ToList();
            return View();
        }

        /// <summary>
        /// Obtener el customerInfoid de los Claims
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Se encarga de retornar los actividades/subprojects filtradas por projectid
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Json de las subproject por projectId</returns>
        [HttpGet]
        public JsonResult GetActivitiesByProjectId(int projectId)
        {
            var results = _context.Subprojects.Where(x => x.ProjectId == projectId).ToList();
            return Json(results);
        }

        /// <summary>
        /// Se encarga de retornar los registros de retro-reflectividad con varios filtros
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="geometryId"></param>
        /// <param name="lineColorId"></param>
        /// <param name="lineNumber"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <param name="pr_init"></param>
        /// <param name="pr_final"></param>
        /// <returns>Json de objetos Reflectivity</returns>
        [HttpGet]
        public JsonResult GetReflectivities(string activities, int? geometryId, int? lineColorId, int? lineNumber, string dateInit,
            string dateFinal, string pr_init, string pr_final)
        {
            var results = GetReflectivitiesByFilters(activities, geometryId, lineColorId, lineNumber, dateInit, dateFinal, pr_init, pr_final, null)
                          .Select(x => new {
                              x.Id,
                              x.Measurement,
                              x.Latitude,
                              x.Longitude,
                              HasPicture = x.Picture != null
                          });
            return Json(results); // ToList() implicito
        }

        /// <summary>
        /// Se encarga de obtener un registro en especifico de reflectividad
        /// y retorna solo los atributos de interes
        /// </summary>
        /// <param name="reflectivityId"></param>
        /// <returns>Json del objeto reflectivity que se desea consultar</returns>
        [HttpGet]
        public async Task<JsonResult> GetReflecitivityById(int reflectivityId)
        {
            var result = await _context.Reflectivities.Include(x => x.Geometry).Include(x => x.LineColor)
                                .Where(x => x.Id == reflectivityId)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.Measurement,
                                    Geometry = x.Geometry.Title,
                                    x.LineNumber,
                                    LineColor = x.LineColor.Title,
                                    x.PrStr,
                                    x.Model,
                                    x.DeviceSerial,
                                    x.ServetDt,
                                    x.DeviceDt,
                                    x.Latitude,
                                    x.Longitude,
                                    x.Picture
                                }).FirstOrDefaultAsync();
            return Json(result);
        }

        /// <summary>
        /// Se encarga de generar el archivo KML de los registros de reflectividad filtrados
        /// </summary>
        /// <param name="activities_kml"></param>
        /// <param name="geometryId_kml"></param>
        /// <param name="lineColorId_kml"></param>
        /// <param name="lineNumber_kml"></param>
        /// <param name="dateInit_kml"></param>
        /// <param name="dateFinal_kml"></param>
        /// <param name="pr_init_kml"></param>
        /// <param name="pr_final_kml"></param>
        [HttpPost]
        public void ExportKml(string activities_kml, int? geometryId_kml, int? lineColorId_kml, int? lineNumber_kml, string dateInit_kml,
            string dateFinal_kml, string pr_init_kml, string pr_final_kml)
        {
            activities_kml = activities_kml.TrimEnd(',');

            var results = GetReflectivitiesByFilters(activities_kml, geometryId_kml, lineColorId_kml, lineNumber_kml, dateInit_kml, dateFinal_kml, pr_init_kml, pr_final_kml, null)
                            .Select(x => new
                            {
                                x.Measurement,
                                x.Latitude,
                                x.Longitude
                            });

            string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nameFileKml = $"Medidas_{dateString}.kml";
            Response.ContentType = "application/vnd.google-earth.kml+xml";
            Response.Headers.Add("Content-Disposition", $"attachment; filename={nameFileKml}");
            XmlTextWriter kml = new XmlTextWriter(HttpContext.Response.Body, System.Text.Encoding.UTF8)
            {
                Formatting = System.Xml.Formatting.Indented,
                Indentation = 3
            };
            kml.WriteStartDocument();

            kml.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");

            kml.WriteStartElement("Document");
            kml.WriteElementString("name", "Medidas");

            kml.WriteStartElement("Folder");
            kml.WriteElementString("name", "Medidas Retroreflectividad Horizontal");
            kml.WriteElementString("open", "1");

            // Recorrer el resultado del filtrado
            foreach(var item in results)
            {
                kml.WriteStartElement("Placemark");
                kml.WriteElementString("name", item.Measurement.ToString());
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

        /// <summary>
        /// Se encarga de generar el archivo Excel de los registros de reflectividad filtrados
        /// </summary>
        /// <param name="activities_xls"></param>
        /// <param name="geometryId_xls"></param>
        /// <param name="lineColorId_xls"></param>
        /// <param name="lineNumber_xls"></param>
        /// <param name="dateInit_xls"></param>
        /// <param name="dateFinal_xls"></param>
        /// <param name="pr_init_xls"></param>
        /// <param name="pr_final_xls"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportExcel(string activities_xls, int? geometryId_xls, int? lineColorId_xls, int? lineNumber_xls, string dateInit_xls,
            string dateFinal_xls, string pr_init_xls, string pr_final_xls, bool include_image_xls)
        {
            activities_xls = activities_xls.TrimEnd(',');
            var results = await GetReflectivitiesByFilters(activities_xls, geometryId_xls, lineColorId_xls, lineNumber_xls, dateInit_xls, dateFinal_xls, pr_init_xls, pr_final_xls, null)
                                .Select(x => new
                                {
                                    x.DeviceDt,
                                    x.Measurement,
                                    Geometry = x.Geometry.Title,
                                    x.LineNumber,
                                    LineColor = x.LineColor.Title,
                                    x.PrStr,
                                    x.Latitude,
                                    x.Longitude,
                                    x.Model,
                                    x.DeviceSerial,
                                    Picture = include_image_xls ? x.Picture : null
                                }).ToListAsync();

            // Generando Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");
                var currentRow = 1;
                // Cabeceros
                worksheet.Cell(currentRow, 1).Value = "Fecha Medición";
                worksheet.Cell(currentRow, 2).Value = "Retro-reflectividad";
                worksheet.Cell(currentRow, 3).Value = "Geometría";
                worksheet.Cell(currentRow, 4).Value = "Línea";
                worksheet.Cell(currentRow, 5).Value = "Color";
                worksheet.Cell(currentRow, 6).Value = "PR";
                worksheet.Cell(currentRow, 7).Value = "Latitud";
                worksheet.Cell(currentRow, 8).Value = "Longitud";
                worksheet.Cell(currentRow, 9).Value = "Modelo Equipo";
                worksheet.Cell(currentRow, 10).Value = "Serial Equipo";
                if (include_image_xls)
                {
                    worksheet.Cell(currentRow, 11).Value = "Imagen";
                }
                // Data
                foreach(var item in results)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.DeviceDt;
                    worksheet.Cell(currentRow, 2).Value = item.Measurement;
                    worksheet.Cell(currentRow, 3).Value = item.Geometry;
                    worksheet.Cell(currentRow, 4).Value = item.LineNumber;
                    worksheet.Cell(currentRow, 5).Value = item.LineColor;
                    worksheet.Cell(currentRow, 6).Value = item.PrStr;
                    worksheet.Cell(currentRow, 7).Value = item.Latitude;
                    worksheet.Cell(currentRow, 8).Value = item.Longitude;
                    worksheet.Cell(currentRow, 9).Value = item.Model;
                    worksheet.Cell(currentRow, 10).Value = item.DeviceSerial;

                    if (include_image_xls)
                    {
                        // Verificar si el registro tiene imagen
                        if (!string.IsNullOrEmpty(item.Picture))
                        {
                            var bytes = Convert.FromBase64String(item.Picture);
                            var ms = new MemoryStream(bytes);
                            var cellImage = worksheet.AddPicture(ms).MoveTo(worksheet.Cell(currentRow, 11));
                            cellImage.Height = 70;
                            cellImage.Width = 60;

                            worksheet.Row(currentRow).Height = 65;
                        }
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string nameFile = $"Medidas_{dateString}.xlsx";
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nameFile);
                }
            }

        }

        [HttpGet]
        public IActionResult ReportPdf()
        {
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Se encarga de obtener los registros de reflectividad y enviarlos a la vista para descargar el PDF (mediante javascript)
        /// </summary>
        /// <param name="activities_pdf"></param>
        /// <param name="geometryId_pdf"></param>
        /// <param name="lineColorId_pdf"></param>
        /// <param name="lineNumber_pdf"></param>
        /// <param name="dateInit_pdf"></param>
        /// <param name="dateFinal_pdf"></param>
        /// <param name="pr_init_pdf"></param>
        /// <param name="pr_final_pdf"></param>
        /// <param name="lineNumbers_pdf"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ReportPdf(string activities_pdf, int? geometryId_pdf, int? lineColorId_pdf, int? lineNumber_pdf, string dateInit_pdf,
            string dateFinal_pdf, string pr_init_pdf, string pr_final_pdf, string lineNumbers_pdf)
        {
            if (string.IsNullOrEmpty(activities_pdf))
            {
                return RedirectToAction(nameof(Index));
            }
            activities_pdf = activities_pdf.TrimEnd(',');

            // Almacenar actividades en Session
            //string activitiesIDs = activities_pdf;
            //HttpContext.Session.SetString("activitiesIDs", activitiesIDs);

            // Filtrar Data
            var results = GetReflectivitiesByFilters(activities_pdf, geometryId_pdf, lineColorId_pdf, lineNumber_pdf, dateInit_pdf, dateFinal_pdf, pr_init_pdf, pr_final_pdf, lineNumbers_pdf)
                                    .Select(x => new Reflectivity {
                                        DeviceDt = x.DeviceDt,
                                        Measurement = x.Measurement,
                                        Geometry = new Geometry { Title = x.Geometry.Title },
                                        LineNumber = x.LineNumber,
                                        LineColor = new LineColor { Title = x.LineColor.Title },
                                        PrStr = x.PrStr,
                                        PrVal = x.PrVal,
                                        Latitude = x.Latitude,
                                        Longitude = x.Longitude,
                                        Model = x.Model,
                                        DeviceSerial = x.DeviceSerial
                                    });

            // VIEWDATAS
            // Filtros para Chart js y Leaftel Map
            ViewData["activities"] = activities_pdf;
            ViewData["geometryId"] = geometryId_pdf;
            ViewData["lineColorId"] = lineColorId_pdf;
            ViewData["lineNumber"] = lineNumber_pdf;
            ViewData["lineNumbers"] = lineNumbers_pdf;
            ViewData["dateInit"] = dateInit_pdf;
            ViewData["dateFinal"] = dateFinal_pdf;
            ViewData["prInit"] = pr_init_pdf;
            ViewData["prFinal"] = pr_final_pdf;

            // CustomerInfo
            int customerInfoId = Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
            var customerInfo = await _context.CustomerInfos.FindAsync(customerInfoId);
            ViewData["customerInfo"] = customerInfo;

            // Devices usados
            var usedDevices = results.Select(x => x.DeviceSerial).Distinct().ToList();
            var devices = await _context.Devices.Include(x => x.Model)
                                .Where(x => usedDevices.Contains(x.AssetSerial)).ToListAsync();
            ViewData["devices"] = devices;

            // Projects and Subprojects
            var subprojectsIds = new List<int>();
            foreach(string id in activities_pdf.Split(','))
            {
                subprojectsIds.Add(Convert.ToInt32(id));
            }
            // Agrupar subprojects por el titulo del proyecto
            var groupSubprojects = await _context.Subprojects.Include(x => x.Project)
                                    .Where(x => subprojectsIds.Contains(x.Id))
                                    .GroupBy(x => x.Project.Title).ToListAsync();
            ViewData["groupSubprojects"] = groupSubprojects; // List<IGrouping<key, group(list subproject)>>

            // PR mínimo y máximo de la consulta
            double prMin = results.Select(x => x.PrVal).Min();
            double prMax = results.Max(x => x.PrVal);
            string prMinStr = await results.Where(x => x.PrVal == prMin).Select(x => x.PrStr).FirstOrDefaultAsync();
            string prMaxStr = await results.Where(x => x.PrVal == prMax).Select(x => x.PrStr).FirstOrDefaultAsync();
            ViewData["prMinStr"] = prMinStr;
            ViewData["prMaxStr"] = prMaxStr;

            return View(results);
        }

        /// <summary>
        /// Se encarga de retornar los registros de retro-reflectividad filtrados
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="geometryId"></param>
        /// <param name="lineColorId"></param>
        /// <param name="lineNumber"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <param name="pr_init"></param>
        /// <param name="pr_final"></param>
        /// <param name="lineNumbers"></param>
        /// <returns></returns>
        [HttpGet]
        public IQueryable<Reflectivity> GetReflectivitiesByFilters(string activities, int? geometryId, int? lineColorId, int? lineNumber, string dateInit,
            string dateFinal, string pr_init, string pr_final, string lineNumbers)
        {
            // Consulta total de Reflectivities
            var query = from r in _context.Reflectivities
                                          .Include(x => x.Subproject)
                                          .Include(x => x.Geometry)
                                          .Include(x => x.LineColor)
                        where r.Deleted == false
                        select r;

            //  APLICANDO FILTROS

            // Actividades - Obtener listados de los ids de los subprojects
            List<int> actividadesIDs = new List<int>();

            // Obtener el filtro de actividades ids para mostrar los marcadores en el mapa
            if (activities.Contains("[") || activities.Contains("{"))
            {
                actividadesIDs = JsonConvert.DeserializeObject<List<ReflectivitySimple>>(activities).Select(x => x.Id).ToList();
            } else
            {
                // Obtener el filtro de activiades ids para los reportes kml, excel y pdf
                foreach (string id in activities.Split(','))
                {
                    actividadesIDs.Add(Convert.ToInt32(id));
                }
            }

            // Emula a la clausula IN
            query = query.Where(x => actividadesIDs.Contains(x.SubprojectId));

            // Line Numbers listado de IDs para el reporte PDF
            if (!string.IsNullOrEmpty(lineNumbers))
            {
                var lineNumbersIDs = new List<int>();
                foreach(string id in lineNumbers.Split(','))
                {
                    lineNumbersIDs.Add(Convert.ToInt32(id));
                }
                query = query.Where(x => lineNumbersIDs.Contains(x.LineNumber));
            } else
            {
                // lineNumber (solo UNO ha sido seleccionado), consultas normales
                if (lineNumber != null)
                {
                    int lineNumberNN = lineNumber.Value;
                    query = query.Where(x => x.LineNumber == lineNumberNN);
                }
            }

            // Filtro de Geometry
            if (geometryId != null)
            {
                int geometryIdNN = geometryId.Value;
                query = query.Where(x => x.GeometryId == geometryIdNN);
            }
            // Filtro de LineColor
            if (lineColorId != null)
            {
                int lineColorIdNN = lineColorId.Value;
                query = query.Where(x => x.LineColorId == lineColorIdNN);
            }
            // Filtro de fecha inicial y final
            if (!string.IsNullOrEmpty(dateInit) && !string.IsNullOrEmpty(dateFinal))
            {
                DateTime inicial = DateTime.ParseExact(dateInit, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime final = DateTime.ParseExact(dateFinal, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                query = from x in query
                        where x.DeviceDt >= inicial && x.DeviceDt <= final
                        select x;
            }
            // PR inicial y final
            if (!string.IsNullOrEmpty(pr_init) && !string.IsNullOrEmpty(pr_final))
            {
                string[] prInitValues = pr_init.Split('+');
                string[] prFinalValues = pr_final.Split('+');
                double prInitValue = (Convert.ToDouble(prInitValues[0]) * 1000) + Convert.ToDouble(prInitValues[1]);
                double prFinalValue = (Convert.ToDouble(prFinalValues[0]) * 1000) + Convert.ToDouble(prFinalValues[1]);
                query = query.Where(x => x.PrVal >= prInitValue && x.PrVal <= prFinalValue);
            }
            // Ordernar consulta por Id
            query = query.OrderBy(x => x.Id);
            return query;
        }

        /// <summary>
        /// Se encarga retornar los registros de reflectividad
        /// agrupa los registros por LineNumber
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="geometryId"></param>
        /// <param name="lineColorId"></param>
        /// <param name="lineNumber"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <param name="pr_init"></param>
        /// <param name="pr_final"></param>
        /// <param name="lineNumbers"></param>
        /// <returns>Json para la creación de los charts en el reporte pdf</returns>
        [HttpGet]
        public async Task<JsonResult> GetDataChart(string activities, int? geometryId, int? lineColorId, int? lineNumber, string dateInit,
            string dateFinal, string pr_init, string pr_final, string lineNumbers)
        {
            try
            {
                // Obtener registros y agrupar por LineNumber
                var results = GetReflectivitiesByFilters(activities, geometryId, lineColorId, lineNumber, dateInit, dateFinal, pr_init, pr_final, lineNumbers)
                                  .GroupBy(x => x.LineNumber)
                                  .OrderBy(x => x.Key)
                                  .Select(x => new LineNumberReport
                                  {  // x.key = lineNubmer -----------  x = group (lista de reflectivities)
                                      LineNumber = x.Key,
                                      AverageMeasurement = x.Select(z => z.Measurement).Average(),
                                      TotalResults = x.Count()
                                  });
                return Json(await results.ToListAsync());
            }
            catch(Exception)
            {
                return Json(null);
            }
        }

        /// <summary>
        /// Obtener latitudes y longitudes de los registros de reflectividad filtrados 
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="geometryId"></param>
        /// <param name="lineColorId"></param>
        /// <param name="lineNumber"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <param name="pr_init"></param>
        /// <param name="pr_final"></param>
        /// <param name="lineNumbers"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetCoordinatesByFilters(string activities, int? geometryId, int? lineColorId, int? lineNumber, string dateInit,
            string dateFinal, string pr_init, string pr_final, string lineNumbers)
        {
            var results = GetReflectivitiesByFilters(activities, geometryId, lineColorId, lineNumber, dateInit, dateFinal, pr_init, pr_final, lineNumbers)
                            .Select(x => new {
                                x.Latitude,
                                x.Longitude
                            });
            return Json(await results.ToListAsync());
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteReflectivity(int id)
        {
            try
            {
                var reflectivity = await _context.Reflectivities.FindAsync(id);
                if (reflectivity == null)
                {
                    return Json(false);
                }
                _context.Reflectivities.Remove(reflectivity);
                await _context.SaveChangesAsync();
                return Json(true);
            }catch(Exception)
            {
                return Json(false);
            }
        }

        /// <summary>
        /// Verificar si el customer user tiene acceso a las vistas del controlador
        /// Si es un usuario administrador entonces se le da acceso sin verificar sus permisos de acceso
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