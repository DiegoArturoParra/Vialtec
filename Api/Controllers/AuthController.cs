using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Utilitarios;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LAccount _logicAccount;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _haccess;

        public AuthController(VialtecContext context, IConfiguration configuration, IHttpContextAccessor haccess)
        {
            _haccess = haccess;
            _configuration = configuration;
            _logicAccount = new LAccount(context);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(UserLogin login)
        {
            // If it is a valid user
            var validation = await IsValidUser(login);
            if (validation.Item1)
            {
                var token = GenerateToken(validation.Item2);
                return Ok(new { token, userDetails = validation.Item2 });
            }

            return NotFound(new { message = "Usuario y/o contraseña incorrectas" });
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUserByToken()
        {
            var claimsIdentity = _haccess.HttpContext.User.Identity as ClaimsIdentity;
            var userClaim = claimsIdentity.FindFirst("user");
            var user = JsonConvert.DeserializeObject<CustomerUser>(userClaim.Value);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        private async Task<(bool, CustomerUser)> IsValidUser(UserLogin login)
        {
            var user = await _logicAccount.Login(login.Email, login.Password);
            if (user == null)
            {
                return (false, user);
            }
            user.PassKey = string.Empty;
            return (user != null, user);
        }

        private string GenerateToken(CustomerUser user)
        {
            // Header
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);

            // Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "user"),
                new Claim("user", JsonConvert.SerializeObject(
                    user, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling  = ReferenceLoopHandling.Ignore }
                ))
            };

            // Payload
            var payload = new JwtPayload
            (
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claims,
                DateTime.Now,
                DateTime.Now.AddYears(1)
            );

            var token = new JwtSecurityToken(header, payload);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}