using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utilitarios;
using Vialtec.Models;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "Customer,CustomerAdmin")]
    public class RadarSpeedsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly VialtecContext _context;
        private readonly LStationarySpeedRadar _logicStationarySpeedRadar;
        private readonly LVehicleType _logicVehicleType;
        private readonly IHostingEnvironment _env;

        public RadarSpeedsController(VialtecContext context, IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            _context = context;
            _configuration = configuration;
            _logicVehicleType = new LVehicleType(context);
            _logicStationarySpeedRadar = new LStationarySpeedRadar(context);
        }

        /// <summary>
        /// Obterner los registros con filtros
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="equipmentId"></param>
        /// <param name="projectId"></param>
        /// <param name="subprojectId"></param>
        /// <param name="speedReportId"></param>
        /// <param name="dateInit"></param>
        /// <param name="dateFinal"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(int? pageNumber, int? equipmentId, int? projectId, int? subprojectId, int? speedReportId, string dateInit, string dateFinal)
        {
            // Número de registros por página
            int pageSize = 50;

            // ViewDatas de las fechas inicial y final
            ViewData["dateInit"] = dateInit ?? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            ViewData["dateFinal"] = dateFinal ?? DateTime.Today.ToString("yyyy-MM-dd");
            ViewData["equipmentId"] = equipmentId;
            // ViewData para los dispositivos con categoría "Radares velocidad"
            ViewData["equipments"] = await _context.Equipments
                                            .Include(x => x.EquipmentGroup).Include(x => x.Device).ThenInclude(x => x.Model)
                                            .Where(x => x.EquipmentGroup.CustomerInfoId == GetCustomerInfoId())
                                            .ToListAsync();

            // ViewData para Proyectos y Actividades
            ViewData["projectId"] = projectId;
            ViewData["projects"] = await _context.Projects.Where(x => x.CustomerInfoId == GetCustomerInfoId()).ToListAsync();
            ViewData["subprojectId"] = subprojectId;

            // ViewData para Reportes de velocidad
            ViewData["speedReportId"] = speedReportId;
            ViewData["speedReports"] = await _context.SpeedReportsCustomer.Where(x => x.CustomerInfoId == GetCustomerInfoId())
                                            .OrderBy(x => x.Id).ToListAsync();

            // La primera vez que entra no le retornamos data porque se necesitan llenar los filtros
            var query = _logicStationarySpeedRadar.All().Where(x => false);

            // Aplicar los filtros
            if (!string.IsNullOrEmpty(dateInit) && !string.IsNullOrEmpty(dateFinal))
            {
                // Definar rango de fechas dependiendo de los filros
                var dtInit = Convert.ToDateTime(dateInit);
                var dtFinal = Convert.ToDateTime(dateFinal).AddDays(1).AddMinutes(-1);

                if (equipmentId != null)
                {
                    // Filtrar por un dispositivo en especifico
                    query = from x in _logicStationarySpeedRadar.All().Include(x => x.VehicleType)
                            let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == speedReportId && z.VehicleTypeId == x.VehicleTypeId)
                            where x.EquipmentId == equipmentId
                            && (subprojectId == null || x.SubprojectId == subprojectId)
                            && x.DeviceDt >= dtInit && x.DeviceDt <= dtFinal
                            orderby x.DeviceDt descending
                            select new StationarySpeedRadar
                            {
                                DeviceDt = x.DeviceDt,
                                ServerDt = x.ServerDt,
                                Speed = x.Speed,
                                EquipmentId = x.EquipmentId,
                                VehicleTypeId = x.VehicleTypeId,
                                VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                            };
                } else
                {
                    // Obtener IDs Dispositivos del cliente actual
                    var equipmentsIDs = await _context.Equipments.Include(x => x.EquipmentGroup)
                                                .Where(x => x.EquipmentGroup.CustomerInfoId == GetCustomerInfoId())
                                                .Select(x => x.Id).ToListAsync();
                    // Filtrar por los dispositivos del cliente actual
                    query = from x in _logicStationarySpeedRadar.All().Include(x => x.VehicleType)
                            let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == speedReportId && z.VehicleTypeId == x.VehicleTypeId)
                            where equipmentsIDs.Contains(x.EquipmentId)
                            && (subprojectId == null || x.SubprojectId == subprojectId)
                            && x.DeviceDt >= dtInit && x.DeviceDt <= dtFinal
                            orderby x.DeviceDt descending
                            select new StationarySpeedRadar
                            {
                                DeviceDt = x.DeviceDt,
                                ServerDt = x.ServerDt,
                                Speed = x.Speed,
                                EquipmentId = x.EquipmentId,
                                VehicleTypeId = x.VehicleTypeId,
                                VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                            };
                }

                // Verificar la cantidad de registros recuperados por la consulta
                int limit = 500000;
                if (query.Count() <= limit)
                {
                    if (equipmentId != null)
                    {
                        var equipment = await _context.Equipments.FindAsync(equipmentId);
                        ViewData["equipmentAlias"] = equipment.EquipmentAlias;
                    }

                    ViewData["dateRange"] = dtInit.ToString("dd/MM/yyyy") + "  -  " + dtFinal.ToString("dd/MM/yyyy");

                    if (subprojectId != null)
                    {
                        var subproject = await _context.Subprojects.FindAsync(subprojectId);
                        ViewData["subprojectTitle"] = subproject.Title;
                    }

                    if (speedReportId != null)
                    {
                        var speedReport = await _context.SpeedReportsCustomer.FindAsync(speedReportId);
                        ViewData["speedReportTitle"] = speedReport.Title;
                    }
                }
                else // Cuando la cantidad es mayor a 500 mil
                {
                    // Limpiar consulta
                    query = query.Where(x => false);
                    ViewData["queryLimit"] = true;
                    // Crear filtro para la consulta
                    var filter = new FilterStationarySpeedRadar
                    {
                        EquipmentId = equipmentId,
                        CustomerInfoId = GetCustomerInfoId(),
                        DateInit = dtInit,
                        DateFinal = dtFinal,
                        SubprojectId = subprojectId,
                        SpeedReportId = speedReportId
                    };
                    // Crear nuevo hilo de ejecución para el envío del correo con los resultados de la consulta
                    Thread hilo = new Thread(new ParameterizedThreadStart(SendDataByEmail));
                    hilo.Start(filter);
                }
            }

            // Total Pages
            ViewData["totalPages"] = (int)Math.Ceiling(query.Count() / (double)pageSize); // 8.3 => 9

            // Pasar el query de EquipmentGroup por el modelo de paginación
            return View(await PaginatedList<StationarySpeedRadar>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        /// <summary>
        /// Obtener el id del cliente
        /// </summary>
        /// <returns></returns>
        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }

        /// <summary>
        /// Obtener el usuario en sesión
        /// </summary>
        /// <returns></returns>
        private string GetCustomerUserEmail()
        {
            return (User.Identity as ClaimsIdentity).Name;
        }

        /// <summary>
        /// Obtener el listado de actividades por proyecto
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetSubprojectsByProjectId(int? projectId)
        {
            var subprojects = await _context.Subprojects.Where(x => x.ProjectId == projectId).ToListAsync();
            return Json(subprojects);
        }

        /// <summary>
        /// El método se encarga de realizar la consulta de los filtros aplicados y exportar el archivo excel
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportExcel(int? equipmentId_xls, string dateInit_xls, string dateFinal_xls, int? subprojectId_xls, int? speedReportId_xls)
        {
            // Definar rango de fechas dependiendo de los filros
            var dtInit = Convert.ToDateTime(dateInit_xls);
            var dtFinal = Convert.ToDateTime(dateFinal_xls).AddDays(1).AddMinutes(-1);

            // Tipear la consulta
            var query = _logicStationarySpeedRadar.All().Where(x => false);

            if (equipmentId_xls != null)
            {
                // Filtrar por un dispositivo en especifico
                query = from x in _logicStationarySpeedRadar.All().Include(x => x.VehicleType)
                        let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == speedReportId_xls && z.VehicleTypeId == x.VehicleTypeId)
                        where x.EquipmentId == equipmentId_xls
                        && (subprojectId_xls == null || x.SubprojectId == subprojectId_xls)
                        && x.DeviceDt >= dtInit && x.DeviceDt <= dtFinal
                        orderby x.DeviceDt descending
                        select new StationarySpeedRadar
                        {
                            DeviceDt = x.DeviceDt,
                            ServerDt = x.ServerDt,
                            Speed = x.Speed,
                            EquipmentId = x.EquipmentId,
                            VehicleTypeId = x.VehicleTypeId,
                            VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                        };
            }
            else
            {
                // Obtener IDs Dispositivos del cliente actual
                var equipmentsIDs = await _context.Equipments.Include(x => x.EquipmentGroup)
                                            .Where(x => x.EquipmentGroup.CustomerInfoId == GetCustomerInfoId())
                                            .Select(x => x.Id).ToListAsync();
                // Filtrar por los dispositivos del cliente actual
                query = from x in _logicStationarySpeedRadar.All().Include(x => x.VehicleType)
                        let custom_vehicle_type = _context.CustomVehicleTypes.FirstOrDefault(z => z.SpeedReportCustomerId == speedReportId_xls && z.VehicleTypeId == x.VehicleTypeId)
                        where equipmentsIDs.Contains(x.EquipmentId)
                        && (subprojectId_xls == null || x.SubprojectId == subprojectId_xls)
                        && x.DeviceDt >= dtInit && x.DeviceDt <= dtFinal
                        orderby x.DeviceDt descending
                        select new StationarySpeedRadar
                        {
                            DeviceDt = x.DeviceDt,
                            ServerDt = x.ServerDt,
                            Speed = x.Speed,
                            EquipmentId = x.EquipmentId,
                            VehicleTypeId = x.VehicleTypeId,
                            VehicleType = custom_vehicle_type == null ? x.VehicleType : new VehicleType { Id = x.VehicleTypeId, Title = custom_vehicle_type.CustomTitle }
                        };
            }

            // Obtener el Stream del archivo Excel generado
            var stream = ManagementWorkbook(query, null);

            // Retornar el Excel para ser descargado
            var content = stream.ToArray();
            string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string nameFile = $"ReporteVelocidadesRadar_{dateString}.xlsx";
            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nameFile);
        }

        /// <summary>
        /// Se encarga de retornar toda la data necesaria para mostrar la gráfica de velocidades por hora
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetDataStationarySpeedRadarByHours(int? equipmentId, string dateInit, string dateFinal, int? subprojectId)
        {
            try
            {
                var dtInit = Convert.ToDateTime(dateInit);
                var dtFinal = Convert.ToDateTime(dateFinal).AddDays(1).AddMinutes(-1);
                var filter = new FilterStationarySpeedRadar {
                    EquipmentId = equipmentId,
                    CustomerInfoId = GetCustomerInfoId(),
                    DateInit = dtInit,
                    DateFinal = dtFinal,
                    SubprojectId = subprojectId
                };
                var results = await _logicStationarySpeedRadar.GetDataStationarySpeedRadarByHours(filter);
                return Json(results);
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Enviar los datos por correo electrónico
        /// </summary>
        /// <param name="obj">Filtro</param>
        private void SendDataByEmail(object obj)
        {
            // Obtener el email del usuario en sesión
            string email = GetCustomerUserEmail();
            // Castear el filtro
            var filter = (FilterStationarySpeedRadar)obj;

            #region "Inicializar DbContext fuera del hilo principal"
            // Builder
            var optionsBuilder = new DbContextOptionsBuilder<VialtecContext>();
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
            // DbContext
            var dbContext = new VialtecContext(optionsBuilder.Options);
            #endregion

            // Lógica de negocio con el DbContext de este hilo
            var logicSSR = new LStationarySpeedRadar(dbContext);
            // Realizar consulta
            var results = logicSSR.All(filter);

            // Generar el archivo excel con los registros de la consulta
            try
            {
                // Guardar archivo temporalmente
                string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
                string nameFile = $"ReporteVelocidadesRadar_{dateString}.xlsx";
                string pathFile = Path.Combine(_env.WebRootPath, "temp_xlsx", nameFile);
                ManagementWorkbook(results, pathFile);

                // Enviar archivo al correo electrónico del usuario
                MailMessage msg = new MailMessage
                {
                    From = new MailAddress("noreply@vialtec.co", "Vialtec"),
                    Subject = "Velocidades de radar",
                    Body = "<span>Reporte de velocidades de radar</span>",
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                // Convertir archivo guardado temporalmente a stream y agregarlo como Attachment al mensaje
                byte[] filebytes = System.IO.File.ReadAllBytes(pathFile);
                Stream stream = new MemoryStream(filebytes);
                msg.Attachments.Add(new Attachment(stream, nameFile));

                msg.To.Add(email);
                // msg.To.Add("blackjackers17@gmail.com");

                // Crear cliente smtp para enviar el mensaje
                SmtpClient client = new SmtpClient
                {
                    Host = "vialtec.co",
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("noreply@vialtec.co", "Wit0ut$3zp0nz3"),
                    EnableSsl = true
                };
                client.Send(msg);

                // Eliminar archivo creado temporalmente porque en este punto ya fue enviado
                System.IO.File.Delete(pathFile);
            }
            catch (SmtpException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Se encarga de almacenar el archivo excel de acuerdo a los resultados de la consulta.
        /// Puede almacenar el archivo en en WebRootPath o 
        /// simplemente retornar el stream del archivo para ser descargado
        /// </summary>
        /// <param name="results"></param>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        private MemoryStream ManagementWorkbook(IEnumerable<StationarySpeedRadar> results, string pathFile)
        {
            // Generando archivo Excel
            using (var workbook = new XLWorkbook())
            {
                #region "HOJA 1"
                var worksheet = workbook.Worksheets.Add("Velocidades de radar");
                var currentRow = 1;
                // Cabeceros
                worksheet.Cell(currentRow, 1).Value = "Velocidad (Km/h)";
                worksheet.Cell(currentRow, 2).Value = "Fecha dispositivo";
                worksheet.Cell(currentRow, 3).Value = "Fecha servidor";
                worksheet.Cell(currentRow, 4).Value = "Categoría";
                // Data Rows
                foreach (var item in results)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Speed;
                    worksheet.Cell(currentRow, 2).Value = item.DeviceDt;
                    worksheet.Cell(currentRow, 3).Value = item.ServerDt;
                    worksheet.Cell(currentRow, 4).Value = item.VehicleType.Title;
                }
                #endregion

                // Generar Stream con el Workbook creado
                var stream = new MemoryStream();
                
                if (!string.IsNullOrEmpty(pathFile))
                {
                    workbook.SaveAs(pathFile);
                } else
                {
                    workbook.SaveAs(stream);
                }

                return stream;
            }
        }
    }
}