using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IStaticDataRepository _staticData;

        public AccountController(IStaticDataRepository staticData)
        {
            _staticData = staticData;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel login)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            var vm = new RegisterViewModel
                         {
                             Schools = _staticData.GetSchools(),
                             Cities = _staticData.GetCities(),
                             Majors = _staticData.GetMajors()
                         };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel login)
        {
            return View();
        }
    }
}
