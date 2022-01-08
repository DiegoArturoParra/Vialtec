using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NpgsqlTypes;
using Utilitarios;
using Vialtec.Models;
using Vialtec.Models.RequestModels;
using Vialtec.Models.ResponseModels;

namespace Vialtec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebaController : ControllerBase
    {
        private readonly VialtecContext _context;

        public PruebaController(VialtecContext context)
        {
            _context = context;
        }

        // GET: api/prueba/login
        [HttpGet("login")]
        public IActionResult Login()
        {
            return Ok(new {  message = "Hola Mundo" });
        }

        private async Task<int> GetCustomerUserIdByLogin(UserModel user)
        {
            int customerUserId = 0;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandText = "customer.validate_customer_user_info"; // function name and schema
                // PARAMETERS
                command.Parameters.Add(new Npgsql.NpgsqlParameter("user_alias", NpgsqlDbType.Text) { Value = user.User });
                // in postgrest tiene la función MD5 encrypt
                command.Parameters.Add(new Npgsql.NpgsqlParameter("pass_key", NpgsqlDbType.Text) { Value = user.Pass });
                if (command.Connection.State == System.Data.ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    customerUserId = reader.GetInt32(0);
                }
                reader.Close();
            }
            return customerUserId;
        }

        // POST: api/prueba/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserModel user)
        {
            if (user.User == null || user.Pass == null)
            {
                return BadRequest(new { result = 2, userId = -1 });
            }
            int customerUserId = await GetCustomerUserIdByLogin(user);
            return Ok(new { result = customerUserId == -1 ? 1 : 0, user_id = customerUserId });
        }

        [HttpPost("projects-customer-user")]
        public async Task<IActionResult> GetProjectsByCustomerUser(UserModel user)
        {
            #region "Bad Request"
            if (user.User == null || user.Pass == null)
            {
                return BadRequest(new { result = 2 });
            }
            int customerUserId = await GetCustomerUserIdByLogin(user);

            if (customerUserId == -1)
            {
                return BadRequest(new { result = 1, });
            }
            #endregion

            var customerUser = await _context.CustomerUsers.FindAsync(customerUserId);
            // Obtener projects para el customerUser
            var projects =  _context.Projects
                                .Where(x => x.CustomerInfoId == customerUser.CustomerInfoId);
            var projectsList = new List<object>();
            // Recorrer projects y agregar sus subprojects
            foreach(var project in projects)
            {
                var item = new
                {
                    project_id = project.Id,
                    project_title = project.Title,
                    activities = _context.Subprojects.Where(x => x.ProjectId == project.Id)
                                 .Select(x => new { activity_id = x.Id, activity_title = x.Title })
                };
                projectsList.Add(item);
            }

            return Ok(new { result = 0, project_list = projectsList });
        }

        [HttpPost("insert-reflectivity")]
        public async Task<IActionResult> InsertReflectivity([FromBody] ReflectivityRequest req)
        {
            #region "Bad Request"
            if (req.user == null || req.pass == null)
            {
                return BadRequest(new { result = 2 });
            }
            int customerUserId = await GetCustomerUserIdByLogin(new UserModel { User = req.user, Pass = req.pass });
            if (customerUserId == -1)
            {
                return BadRequest(new { result = 1, });
            }

            // Verificar que las propiedades necesarias no vengan nulas
            if (req.network_identifier == null || req.datetime == null || req.latitude == null || req.longitude == null
                || req.pr_km == null || req.pr_mt == null || req.color == null || req.line == null || req.measurement == null
                || req.serial == null || req.activity == null || req.model == null || req.geometry == null)
            {
                return BadRequest(new { result = 2 });
            }
            #endregion

            // Insertar nuevo registro de reflectividad
            int response = -1;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                try
                {
                    #region "Prepara Command"
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "transmission.insert_reflectivity_measurement"; // function name and schema
                                                                                          // PARAMETERS
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("network_identifier", NpgsqlDbType.Text) { Value = req.network_identifier });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("event_dt", NpgsqlDbType.Text) { Value = req.datetime });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("raw_message", NpgsqlDbType.Text) { Value = JsonConvert.SerializeObject(req) });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("latitude", NpgsqlDbType.Double) { Value = req.latitude });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("longitude", NpgsqlDbType.Double) { Value = req.longitude });
                    var prVal = (req.pr_km * 1000.0) + req.pr_mt;
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("pr_val", NpgsqlDbType.Double) { Value = prVal });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("pr_str", NpgsqlDbType.Text) { Value = $"{req.pr_km}+{req.pr_mt}" });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("line_color", NpgsqlDbType.Integer) { Value = req.color });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("line_number", NpgsqlDbType.Integer) { Value = req.line });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("measurement", NpgsqlDbType.Integer) { Value = req.measurement });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("device_serial", NpgsqlDbType.Text) { Value = req.serial });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("activity", NpgsqlDbType.Integer) { Value = req.activity });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("model", NpgsqlDbType.Text) { Value = req.model });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("geometry_type", NpgsqlDbType.Integer) { Value = req.geometry });
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("picture", NpgsqlDbType.Text) { Value = req.picture ?? "" });
                    #endregion

                    if (command.Connection.State == System.Data.ConnectionState.Closed)
                    {
                        command.Connection.Open();
                    }

                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        response = reader.GetInt32(0);
                    }
                    reader.Close();
                }
                catch(Exception)
                {
                    return BadRequest(new { result = 4 });
                }
            }

            return Ok(new { result = response });
        }

        [HttpPost("equipments-customer-user")]
        public async Task<IActionResult> GetEquipmentsByCustomerUser(UserModel user)
        {
            #region "Bad Request"
            if (user.User == null || user.Pass == null || user.Category_Type == null)
            {
                return BadRequest(new { result = 2 });
            }
            int customerUserId = await GetCustomerUserIdByLogin(user);

            if (customerUserId == -1)
            {
                return BadRequest(new { result = 1, });
            }
            #endregion

            var customerUser = await _context.CustomerUsers.FindAsync(customerUserId);

            // Obtener los equipments por CustomerInfo
            var equipments = _context.Equipments
                                .Include(x => x.EquipmentGroup)
                                .Include(x => x.Device)
                                .ThenInclude(d => d.Model)
                                .ThenInclude(m => m.Category)
                                .Where(x => x.EquipmentGroup.CustomerInfoId == customerUser.CustomerInfoId)
                                .Where(x => x.Device.Model.CategoryId == user.Category_Type);
            // Nueva lista para personalizar la respuesta
            var equipmentList = new List<EquipmentsResponse>();
            // Recorrer los equipments de la consulta
            foreach(var equip in equipments)
            {
                var item = new EquipmentsResponse
                {
                    equipment_id = equip.Id,
                    equipment_alias = equip.EquipmentAlias,
                    equipment_description = equip.Description,
                    network_identifier = equip.Device.NetworkIdentifier,
                    asset_serial = equip.Device.AssetSerial,
                    model_id = equip.Device.ModelId,
                    model_title = equip.Device.Model.Title,
                    dev_pass = equip.Device.DevPass
                };
                if (string.IsNullOrEmpty(equip.Description))
                {
                    item.security_data = null;
                }
                // Bluetooth info parse JSON
                try
                {
                    var bluetoothInfoJson = JsonConvert.DeserializeObject(equip.Device.BluetoothInfo);
                    item.bluetooth_info = bluetoothInfoJson;
                }
                catch (Exception)
                {
                    item.bluetooth_info = null;
                }
                equipmentList.Add(item);
            }

            return Ok(new { result = 0, device_list = equipmentList });
        }
    }
}