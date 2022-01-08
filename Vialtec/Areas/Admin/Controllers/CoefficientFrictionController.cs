using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using ExcelDataReader;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Utilitarios;

namespace Vialtec.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoefficientFrictionController : Controller
    {
        private readonly LProject _logicProject;
        private readonly VialtecContext _context;
        private readonly LSubproject _logicSubproject;
        private readonly LCustomerInfo _logicCustomerInfo;
        private readonly LCoefficientFriction _logicCoefficientFriction;

        public CoefficientFrictionController(VialtecContext context)
        {
            _context = context;
            _logicProject = new LProject(context);
            _logicSubproject = new LSubproject(context);
            _logicCustomerInfo = new LCustomerInfo(context);
            _logicCoefficientFriction = new LCoefficientFriction(context);
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            ViewData["customersInfo"] = await _logicCustomerInfo.All()
                                        .Where(x => x.DistributorInfoId == GetDistributorInfoId()).ToListAsync();
            return View();
        }

        /// <summary>
        /// Obtener los proyectos del cliente
        /// </summary>
        /// <param name="customerInfoId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetProjectsByCustomerInfo(int customerInfoId)
        {
            var projects = await _logicProject.All()
                                .Where(x => x.CustomerInfoId == customerInfoId).ToListAsync();
            return Json(projects);
        }

        /// <summary>
        /// Obtener las actividades del proyecto seleccionado
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetSubprojectsByProject(int projectId)
        {
            var subprojects = await _logicSubproject.All()
                                .Where(x => x.ProjectId == projectId).ToListAsync();
            return Json(subprojects);
        }

        /// <summary>
        /// Subida del archivo y asignación de columnas
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile uploadFile, string columns, int customerInfoId, string muReportName)
        {
            var columnsList = JsonConvert.DeserializeObject<List<string>>(columns);
            var coefficientFrictionList = new List<CoefficientFriction>();
            if (uploadFile != null)
            {
                var fileExtension = Path.GetExtension(uploadFile.FileName);
                if (fileExtension.ToLower().Equals(".xls") || fileExtension.ToLower().Equals(".xlsx") || fileExtension.ToLower().Equals(".csv"))
                {
                    try
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        using (var stream = new MemoryStream())
                        {
                            uploadFile.CopyTo(stream);
                            stream.Position = 0;
                            // Seleccionar ExcelReaderFactory dependiendo del tipo de archivo Excel/CSV
                            using (var reader = (!fileExtension.ToLower().Equals(".csv") ? ExcelReaderFactory.CreateReader(stream) : ExcelReaderFactory.CreateCsvReader(stream)))
                            {
                                int i = 0;
                                while (reader.Read()) //Each row of the file
                                {
                                    if (i == 0) // Descartar la fila de los encabezados
                                    {
                                        i++;
                                        continue;
                                    }
                                    var coefficientFriction = new CoefficientFriction
                                    {
                                        Latitude = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("latitude")).ToString()),
                                        Longitude = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("longitude")).ToString()),
                                        Mu = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("mu")).ToString()),
                                        Odometer = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("odometer")).ToString()),
                                        Date = null,
                                        TemperatureVia = 0,
                                        TemperatureEnvironment = 0,
                                        Speed = 0,
                                        PrStr = "empty"
                                        //Date = Convert.ToDateTime(reader.GetValue(columnsList.IndexOf("date")).ToString()),
                                        //TemperatureVia = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("tempVia")).ToString()),
                                        //TemperatureEnvironment = Convert.ToDouble(reader.GetValue(columnsList.IndexOf("tempEnv")).ToString()),
                                        //Speed = Convert.ToInt32(reader.GetValue(columnsList.IndexOf("speed")).ToString()),
                                        //PrStr = reader.GetValue(columnsList.IndexOf("pr")).ToString()
                                    };
                                    coefficientFrictionList.Add(coefficientFriction);
                                }
                            }

                            // Guardar Mu_Report primero
                            var muReport = new MuReport { CustomerInfoId = customerInfoId, Title = muReportName };
                            _context.MuReports.Add(muReport);
                            await _context.SaveChangesAsync();

                            // Agregar el MuReportId a los registros agregados
                            coefficientFrictionList.ForEach(x => x.MuReportId = muReport.Id);

                            // Guardar el listado de registros en la base de datos
                            var res = await _logicCoefficientFriction.AddRange(coefficientFrictionList);
                            ViewData["success-message"] = "Los datos han sido cargados exitosamente.";
                        }
                    }
                    catch(Exception ex)
                    {
                        ViewData["error-message"] = ex.Message;
                    }
                }
            }
            ViewData["customersInfo"] = await _logicCustomerInfo.All()
                            .Where(x => x.DistributorInfoId == GetDistributorInfoId()).ToListAsync();
            return View();
        }

        /// <summary>
        /// Obtener el customerInfoId del cliente actual
        /// </summary>
        /// <returns></returns>
        public int GetDistributorInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("distributorInfoId").Value);
        }
    }
}