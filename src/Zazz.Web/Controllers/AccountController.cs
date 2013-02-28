﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IStaticDataRepository _staticData;
        private readonly IAuthService _authService;

        public AccountController(IStaticDataRepository staticData, IAuthService authService)
        {
            _staticData = staticData;
            _authService = authService;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel login, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _authService.LoginAsync(login.Username, login.Password);

                    FormsAuthentication.SetAuthCookie(login.Username, true);

                    if (!String.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
                catch (InvalidPasswordException)
                {
                    ShowAlert("Invalid login information.", AlertType.Error);
                }
                catch (UserNotExistsException)
                {
                    ShowAlert("Invalid login information.", AlertType.Warning);
                }
            }

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

        public ActionResult Signout()
        {
            if (User.Identity.IsAuthenticated)
                FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
