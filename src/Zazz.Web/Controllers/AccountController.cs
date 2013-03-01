using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Web.Models;
using OAuthAccount = Zazz.Core.Models.Data.OAuthAccount;

namespace Zazz.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IStaticDataRepository _staticData;
        private readonly IAuthService _authService;
        private readonly ICryptoService _cryptoService;

        public AccountController(IStaticDataRepository staticData, IAuthService authService, ICryptoService cryptoService)
        {
            _staticData = staticData;
            _authService = authService;
            _cryptoService = cryptoService;
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
                    ShowAlert("Invalid login information.", AlertType.Warning);
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
            if (User.Identity.IsAuthenticated)
                RedirectToAction("Index", "Home");

            var vm = new RegisterViewModel
                         {
                             Schools = _staticData.GetSchools(),
                             Cities = _staticData.GetCities(),
                             Majors = _staticData.GetMajors()
                         };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel registerVm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                               {
                                   CityId = registerVm.CityId,
                                   Email = registerVm.Email,
                                   JoinedDate = DateTime.UtcNow,
                                   IsConfirmed = false,
                                   LastActivity = DateTime.UtcNow,
                                   FullName = registerVm.FullName,
                                   MajorId = registerVm.MajorId,
                                   Password = registerVm.Password,
                                   PublicEmail = registerVm.PublicEmail,
                                   SchoolId = registerVm.SchoolId,
                                   Username = registerVm.UserName
                               };

                try
                {
                    await _authService.RegisterAsync(user, true);

                    //TODO: send welcome email.

                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    return RedirectToAction("Index", "Home");
                }
                catch (UsernameExistsException)
                {
                    ShowAlert("Username is already exists.", AlertType.Warning);
                }
                catch (EmailExistsException)
                {
                    ShowAlert("This email address has registered before. Please login.", AlertType.Warning);
                }
            }

            registerVm.Schools = _staticData.GetSchools();
            registerVm.Cities = _staticData.GetCities();
            registerVm.Majors = _staticData.GetMajors();

            return View(registerVm);
        }

        internal class OAuthLoginResult : ActionResult
        {
            public OAuthLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        public ActionResult OAuth(string id)
        {
            return new OAuthLoginResult(id, "/account/oauthcallback");
        }

        public async Task<ActionResult> OAuthCallback()
        {
            var result = OAuthWebSecurity.VerifyAuthentication(Url.Action("OAuthCallback"));
            if (!result.IsSuccessful)
            {
                ShowAlert("Operation was not successful. Please try again later.", AlertType.Warning);
                return View("Login");
            }

            var oauthVersion = OAuthVersion.Two; //TODO : assign the correct version.
            
            OAuthProvider provider;
            if (!OAuthProvider.TryParse(result.Provider, true, out provider))
                throw new Exception("Unable to validate the provider");

            var providerId = result.ProviderUserId;
            var name = result.UserName;
            var email = result.ExtraData["email"];
            var accessToken = result.ExtraData["accesstoken"];

            var oauthAccount = new Zazz.Core.Models.Data.OAuthAccount
                                   {
                                       AccessToken = accessToken,
                                       OAuthVersion = oauthVersion,
                                       Provider = provider,
                                       ProviderUserId = long.Parse(providerId)
                                   };

            var user = await _authService.GetOAuthUserAsync(oauthAccount, email);
            if (user != null)
            {
                //user exists
                FormsAuthentication.SetAuthCookie(user.Username, true);
            }
            else
            {
                var oauthData = new OAuthLoginResponse
                                    {
                                        AccessToken = accessToken,
                                        Email = email,
                                        Name = name,
                                        Provider = provider,
                                        ProviderUserId = long.Parse(providerId),
                                        OAuthVersion = oauthVersion
                                    };

                TempData["oauthData"] = oauthData;
                return RedirectToAction("OAuthRegister");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult OAuthRegister()
        {
            var oAuthResponse = TempData["oauthData"] as OAuthLoginResponse;

            var jsonData = JsonConvert.SerializeObject(oAuthResponse, Formatting.None);
            var jsonSign = _cryptoService.GenerateTextSignature(jsonData);

            var registerPageVM = new OAuthRegisterViewModel
            {
                Email = oAuthResponse.Email,
                FullName = oAuthResponse.Name,
                OAuthProvidedData = jsonData,
                ProvidedDataSignature = jsonSign,
                Cities = _staticData.GetCities(),
                Majors = _staticData.GetMajors(),
                Schools = _staticData.GetSchools()
            };

            return View("OAuthRegister", registerPageVM);
        }

        [HttpPost]
        public async Task<ActionResult> OAuthRegister(OAuthRegisterViewModel registerVm)
        {
            if (ModelState.IsValid)
            {
                var oauthDataSignature = _cryptoService.GenerateTextSignature(registerVm.OAuthProvidedData);
                if (oauthDataSignature != registerVm.ProvidedDataSignature)
                    throw new SecurityException("Unable to verify data integrity.");

                var oauthData = JsonConvert.DeserializeObject<OAuthLoginResponse>(registerVm.OAuthProvidedData);

                var user = new User
                {
                    CityId = registerVm.CityId,
                    Email = oauthData.Email,
                    JoinedDate = DateTime.UtcNow,
                    IsConfirmed = true,
                    LastActivity = DateTime.UtcNow,
                    FullName = registerVm.FullName,
                    MajorId = registerVm.MajorId,
                    Password = registerVm.Password,
                    PublicEmail = registerVm.PublicEmail,
                    SchoolId = registerVm.SchoolId,
                    Username = registerVm.UserName,
                };
                user.LinkedAccounts = new List<OAuthAccount>
                                          {
                                              new OAuthAccount
                                                  {
                                                      AccessToken = oauthData.AccessToken,
                                                      OAuthVersion = oauthData.OAuthVersion,
                                                      Provider = oauthData.Provider,
                                                      ProviderUserId = oauthData.ProviderUserId
                                                  }
                                          };
                try
                {
                    await _authService.RegisterAsync(user, false);

                    //TODO: send welcome email.

                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    return RedirectToAction("Index", "Home");
                }
                catch (UsernameExistsException)
                {
                    ShowAlert("Username is already exists.", AlertType.Warning);
                }
                catch (EmailExistsException)
                {
                    ShowAlert("Sorry we're unable to register you at this moment. Please contact us.", AlertType.Warning);
                    return RedirectToAction("Index", "Home");
                }
            }

            registerVm.Cities = _staticData.GetCities();
            registerVm.Schools = _staticData.GetSchools();
            registerVm.Majors = _staticData.GetMajors();

            return View(registerVm);
        }

        public ActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
                FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
