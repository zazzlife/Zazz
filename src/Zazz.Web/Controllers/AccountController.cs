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
using Zazz.Core.Models.Data.Enums;
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

        public AccountController(IStaticDataRepository staticData, IAuthService authService,
            ICryptoService cryptoService, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper) 
            : base(userService, photoService, defaultImageHelper)
        {
            _staticData = staticData;
            _authService = authService;
            _cryptoService = cryptoService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel login, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    _authService.Login(login.Username, login.Password);

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
                return RedirectToAction("Index", "Home");

            if (User.Identity.IsAuthenticated)
                RedirectToAction("Index", "Home");

            var vm = new RegisterViewModel
                         {
                             Schools = _staticData.GetSchools(),
                             Cities = _staticData.GetCities(),
                             Majors = _staticData.GetMajors(),
                         };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel registerVm)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var user = new User
                               {
                                   Email = registerVm.Email,
                                   IsConfirmed = false,
                                   LastActivity = DateTime.UtcNow,
                                   Username = registerVm.UserName,
                                   AccountType = registerVm.AccountType,
                                   JoinedDate = DateTime.UtcNow,
                                   Preferences = new UserPreferences
                                                 {
                                                     SendSyncErrorNotifications = true,
                                                     SyncFbEvents = true,
                                                     SyncFbImages = registerVm.AccountType == AccountType.Club,
                                                     SyncFbPosts = registerVm.AccountType == AccountType.Club
                                                 }
                               };

                if (registerVm.AccountType == AccountType.Club)
                {
                    user.ClubDetail = new ClubDetail
                                      {
                                          ClubName = registerVm.ClubName,
                                          Address = registerVm.ClubAddress,
                                          ClubType = registerVm.ClubType,
                                      };
                }
                else
                {
                    user.UserDetail = new UserDetail
                                      {
                                          Gender = registerVm.Gender,
                                          PublicEmail = registerVm.PublicEmail,
                                          SchoolId = registerVm.SchoolId,
                                          FullName = registerVm.FullName,
                                          MajorId = registerVm.MajorId,
                                          CityId = registerVm.CityId,
                                      };
                }

                try
                {
                    _authService.Register(user, registerVm.Password, true);
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

        [HttpGet]
        public ActionResult Recover()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Recover(string email)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var token = _authService.GenerateResetPasswordToken(email);

                    var resetLink = String.Format("/account/resetpassword/{0}/{1}", token.Id, token.Token.ToString());
                    var message = String.Format(
                        "A recovery email has been sent to {0}. Please check your inbox.{1}{2}", email,
                        Environment.NewLine, "test: " + resetLink);

                    ShowAlert(message, AlertType.Success);

                }
                catch (EmailNotExistsException)
                {
                    ShowAlert("Email not found", AlertType.Warning);
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(int? id, Guid? token)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (!id.HasValue || !token.HasValue)
                throw new HttpException(404, "Requested url is not valid");

            try
            {
                var isTokenValid = _authService.IsTokenValid(id.Value, token.Value);
                if (!isTokenValid)
                    throw new HttpException(404, "Requested url is not valid");
            }
            catch (TokenExpiredException)
            {
                ShowAlert("This token has been expired. Please request a new one.", AlertType.Warning);
                return RedirectToAction("Recover");
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, Guid token, ResetPasswordModel resetPasswordModel)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _authService.ResetPassword(id, token, resetPasswordModel.NewPassword);
                ShowAlert("Your password has been successfully changed.", AlertType.Success);

                return RedirectToAction("Login");
            }

            return View();
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

        public ActionResult OAuthCallback()
        {
            var result = OAuthWebSecurity.VerifyAuthentication(Url.Action("OAuthCallback"));
            if (!result.IsSuccessful)
            {
                ShowAlert("Operation was not successful. Please try again later.", AlertType.Warning);
                return View("Login");
            }

            OAuthProvider provider;
            if (!OAuthProvider.TryParse(result.Provider, true, out provider))
                throw new Exception("Unable to validate the provider");

            var providerId = result.ProviderUserId;
            var name = result.UserName;
            var email = result.ExtraData["email"];
            var accessToken = result.ExtraData["accesstoken"];

            var oauthAccount = new OAuthAccount
                                   {
                                       AccessToken = accessToken,
                                       Provider = provider,
                                       ProviderUserId = long.Parse(providerId)
                                   };

            var user = _authService.GetOAuthUser(oauthAccount, email);

            if (User.Identity.IsAuthenticated)
            {
                // the user is already signed in but oauth email is different
                var userId = UserService.GetUserId(User.Identity.Name);
                oauthAccount.UserId = userId;

                _authService.AddOAuthAccount(oauthAccount);
            }
            else
            {
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
                    };

                    TempData["oauthData"] = oauthData;
                    return RedirectToAction("OAuthRegister");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult OAuthRegister()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

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
                Schools = _staticData.GetSchools(),
            };

            return View("OAuthRegister", registerPageVM);
        }

        [HttpPost]
        public ActionResult OAuthRegister(OAuthRegisterViewModel registerVm)
        {
            if (ModelState.IsValid)
            {
                var oauthDataSignature = _cryptoService.GenerateTextSignature(registerVm.OAuthProvidedData);
                if (oauthDataSignature != registerVm.ProvidedDataSignature)
                    throw new SecurityException("Unable to verify data integrity.");

                var oauthData = JsonConvert.DeserializeObject<OAuthLoginResponse>(registerVm.OAuthProvidedData);

                var user = new User
                           {
                               Email = registerVm.Email,
                               IsConfirmed = true,
                               LastActivity = DateTime.UtcNow,
                               Username = registerVm.UserName,
                               AccountType = registerVm.AccountType,
                               JoinedDate = DateTime.UtcNow,
                               Preferences = new UserPreferences
                                             {
                                                 SendSyncErrorNotifications = true,
                                                 SyncFbEvents = true,
                                                 SyncFbImages = registerVm.AccountType == AccountType.Club,
                                                 SyncFbPosts = registerVm.AccountType == AccountType.Club
                                             }
                           };

                if (registerVm.AccountType == AccountType.Club)
                {
                    user.ClubDetail = new ClubDetail
                                      {
                                          ClubName = registerVm.ClubName,
                                          Address = registerVm.ClubAddress,
                                          ClubType = registerVm.ClubType,
                                      };
                }
                else
                {
                    user.UserDetail = new UserDetail
                                      {
                                          Gender = registerVm.Gender,
                                          PublicEmail = registerVm.PublicEmail,
                                          SchoolId = registerVm.SchoolId,
                                          FullName = registerVm.FullName,
                                          MajorId = registerVm.MajorId,
                                          CityId = registerVm.CityId
                                      };
                }

                user.LinkedAccounts = new List<OAuthAccount>
                                          {
                                              new OAuthAccount
                                                  {
                                                      AccessToken = oauthData.AccessToken,
                                                      Provider = oauthData.Provider,
                                                      ProviderUserId = oauthData.ProviderUserId
                                                  }
                                          };
                try
                {
                    _authService.Register(user, registerVm.Password, false);
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
