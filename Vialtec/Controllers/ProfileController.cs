using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Datos;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Utilitarios;

namespace Vialtec.Controllers
{
    [Authorize(Roles = "CustomerAdmin")]
    public class ProfileController : Controller
    {
        private readonly LCustomerInfo _logicCustomerInfo;

        public ProfileController(VialtecContext context)
        {
            _logicCustomerInfo = new LCustomerInfo(context);
        }

        public async Task<IActionResult> Index()
        {
            var customerInfo = await _logicCustomerInfo.Find(GetCustomerInfoId());
            return View(customerInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("Id,LogoBase64")] CustomerInfo customerInfo)
        {
            try
            {
                var model = await _logicCustomerInfo.Find(customerInfo.Id);
                model.LogoBase64 = customerInfo.LogoBase64.Split(',')[1];
                await _logicCustomerInfo.Update(model);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                return View(customerInfo);
            }
        }

        private int GetCustomerInfoId()
        {
            return Convert.ToInt32((User.Identity as ClaimsIdentity).FindFirst("customerInfoId").Value);
        }
    }
}