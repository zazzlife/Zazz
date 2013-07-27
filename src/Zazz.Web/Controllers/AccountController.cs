using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json;
using PoliteCaptcha;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ICryptoService _cryptoService;
        private readonly IObjectMapper _objectMapper;
        private readonly IFacebookService _facebookService;
        private readonly IFollowService _followService;
        private readonly IUoW _uow;
        private readonly IOAuthService _oAuthService;

        private const string IS_MOBILE_SESSION_KEY = "IsMobile";
        private const string IS_OAUTH_KEY = "IsOAuth";
        private const string OAUTH_PROVIDER_KEY = "OAUTH_Provider";
        private const string OAUTH_PROVIDER_USERID_KEY = "OAUTH_ProviderUserId";
        private const string OAUTH_EMAIL_KEY = "OAUTH_Email";
        private const string OAUTH_ACCESS_TOKEN_KEY = "OAUTH_AccessToken";

        public AccountController(IStaticDataRepository staticData, IAuthService authService,
            ICryptoService cryptoService, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper, IObjectMapper objectMapper,IFacebookService facebookService,
            IFollowService followService, IUoW uow, IOAuthService oAuthService) 
            : base(userService, photoService, defaultImageHelper, staticData)
        {
            _authService = authService;
            _cryptoService = cryptoService;
            _objectMapper = objectMapper;
            _facebookService = facebookService;
            _followService = followService;
            _uow = uow;
            _oAuthService = oAuthService;
        }

        public JsonNetResult IsAvailable(string username)
        {
            if (String.IsNullOrWhiteSpace(username) || username.Length < 2 || username.Length > 20)
                return new JsonNetResult(false);

            var usernameExists = _uow.UserRepository.ExistsByUsername(username);
            return usernameExists ? new JsonNetResult(false) : new JsonNetResult(true);
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

        public ActionResult SessionExpired()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SelectAccountType()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RegisterUser()
        {
            var vm = new RegisterUserViewModel
            {
                Schools = StaticDataRepository.GetSchools(),
                Majors = StaticDataRepository.GetMajors(),
                Cities = StaticDataRepository.GetCities(),
                Gender = Gender.NotSpecified
            };


            if (Session[IS_OAUTH_KEY] != null && ((bool) Session[IS_OAUTH_KEY]))
            {
                var email = (string)Session[OAUTH_EMAIL_KEY];

                vm.IsOAuth = true;
                vm.Email = email;
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult RegisterUser(RegisterUserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                           {
                               Username = vm.UserName,
                               AccountType = AccountType.User,
                               Email = vm.Email,
                               IsConfirmed = false,
                               JoinedDate = DateTime.UtcNow,
                               LastActivity = DateTime.UtcNow,
                               Preferences = new UserPreferences
                                             {
                                                 SyncFbEvents = true,
                                                 SyncFbImages = false,
                                                 SyncFbPosts = false,
                                                 SendSyncErrorNotifications = true
                                             },
                               UserDetail = new UserDetail
                                            {
                                                CityId = vm.CityId,
                                                FullName = vm.FullName,
                                                Gender = vm.Gender,
                                                MajorId = vm.MajorId,
                                                SchoolId = vm.SchoolId
                                            }
                           };

                var isOAuth = (bool?)Session[IS_OAUTH_KEY];
                if (isOAuth.HasValue && isOAuth.Value)
                {
                    try
                    {
                        var provider = (OAuthProvider) Session[OAUTH_PROVIDER_KEY];
                        var providerUserId = Int64.Parse((string) Session[OAUTH_PROVIDER_USERID_KEY]);
                        var email = (string) Session[OAUTH_EMAIL_KEY];
                        var accessToken = (string) Session[OAUTH_ACCESS_TOKEN_KEY];

                        if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(accessToken))
                            throw new ApplicationException("Session expired!");

                        user.IsConfirmed = true;
                        user.Email = email;

                        user.LinkedAccounts.Add(new LinkedAccount
                                                {
                                                    AccessToken = accessToken,
                                                    Provider = provider,
                                                    ProviderUserId = providerUserId
                                                });
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("SessionExpired");
                    }
                    finally
                    {
                        Session.Remove(IS_OAUTH_KEY);
                        Session.Remove(OAUTH_PROVIDER_KEY);
                        Session.Remove(OAUTH_PROVIDER_USERID_KEY);
                        Session.Remove(OAUTH_EMAIL_KEY);
                        Session.Remove(OAUTH_ACCESS_TOKEN_KEY);
                    }
                }

                try
                {
                    _authService.Register(user, vm.Password, !user.IsConfirmed);
                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
                    if (isMobile.HasValue && isMobile.Value)
                    {
                        Session.Remove(IS_MOBILE_SESSION_KEY);
                        return HandleMobileClientOAuthCallback(user);
                    }
                    else if (isOAuth.HasValue && isOAuth.Value)
                    {
                        return RedirectToAction("FindFriends");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (InvalidEmailException)
                {
                    ModelState.AddModelError("Email", "Invalid email address");
                }
                catch (PasswordTooLongException)
                {
                    ModelState.AddModelError("Password", "Password too long");
                }
                catch (UsernameExistsException)
                {
                    ModelState.AddModelError("Username", "Username not available");
                }
                catch (EmailExistsException)
                {
                    ModelState.AddModelError("Email", "Email address already registered");
                }
            }
            
            vm.Schools = StaticDataRepository.GetSchools();
            vm.Majors = StaticDataRepository.GetMajors();
            vm.Cities = StaticDataRepository.GetCities();

            return View(vm);
        }

        [HttpGet]
        public ActionResult RegisterClub()
        {
            var vm = new RegisterClubViewModel();

            if (Session[IS_OAUTH_KEY] != null && ((bool)Session[IS_OAUTH_KEY]))
            {
                var email = (string)Session[OAUTH_EMAIL_KEY];

                vm.IsOAuth = true;
                vm.Email = email;
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult RegisterClub(RegisterClubViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = vm.UserName,
                    AccountType = AccountType.Club,
                    Email = vm.Email,
                    IsConfirmed = false,
                    JoinedDate = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    Preferences = new UserPreferences
                    {
                        SyncFbEvents = true,
                        SyncFbImages = true,
                        SyncFbPosts = true,
                        SendSyncErrorNotifications = true
                    },
                    ClubDetail = new ClubDetail
                                 {
                                     Address = vm.ClubAddress,
                                     ClubName = vm.ClubName,
                                     ClubType = vm.ClubType,
                                 }
                };

                var isOAuth = (bool?) Session[IS_OAUTH_KEY];
                if (isOAuth.HasValue && isOAuth.Value)
                {
                    try
                    {
                        var provider = (OAuthProvider)Session[OAUTH_PROVIDER_KEY];
                        var providerUserId = Int64.Parse((string)Session[OAUTH_PROVIDER_USERID_KEY]);
                        var email = (string)Session[OAUTH_EMAIL_KEY];
                        var accessToken = (string)Session[OAUTH_ACCESS_TOKEN_KEY];

                        if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(accessToken))
                            throw new ApplicationException("Session expired!");

                        user.IsConfirmed = true;
                        user.Email = email;

                        user.LinkedAccounts.Add(new LinkedAccount
                        {
                            AccessToken = accessToken,
                            Provider = provider,
                            ProviderUserId = providerUserId
                        });
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("SessionExpired");
                    }
                    finally
                    {
                        Session.Remove(IS_OAUTH_KEY);
                        Session.Remove(OAUTH_PROVIDER_KEY);
                        Session.Remove(OAUTH_PROVIDER_USERID_KEY);
                        Session.Remove(OAUTH_EMAIL_KEY);
                        Session.Remove(OAUTH_ACCESS_TOKEN_KEY);
                    }
                }

                try
                {
                    _authService.Register(user, vm.Password, !user.IsConfirmed);
                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
                    if (isMobile.HasValue && isMobile.Value)
                    {
                        Session.Remove(IS_MOBILE_SESSION_KEY);
                        return HandleMobileClientOAuthCallback(user);
                    }
                    else if (isOAuth.HasValue && isOAuth.Value)
                    {
                        return RedirectToAction("FindFriends");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (InvalidEmailException)
                {
                    ModelState.AddModelError("Email", "Invalid email address");
                }
                catch (PasswordTooLongException)
                {
                    ModelState.AddModelError("Password", "Password too long");
                }
                catch (UsernameExistsException)
                {
                    ModelState.AddModelError("Username", "Username not available");
                }
                catch (EmailExistsException)
                {
                    ModelState.AddModelError("Email", "Email address already registered");
                }
            }

            return View(vm);
        }

        private ActionResult HandleMobileClientOAuthCallback(User user)
        {
            var scopes = StaticDataRepository.GetOAuthScopes()
                .Where(s => s.Name.Equals("full", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            var client = StaticDataRepository.GetOAuthClients()
                .Single(c => c.Name.Equals("Zazz", StringComparison.InvariantCultureIgnoreCase));

            var creds = _oAuthService.CreateOAuthCredentials(user, client, scopes);

            var queryString = HttpUtility.ParseQueryString(String.Empty);
            queryString["access_token"] = creds.AccessToken.ToJWTString();
            queryString["refresh_token"] = creds.RefreshToken.ToJWTString();

            var url = "/loginSuccess#" + queryString.ToString();
            return Redirect(url);
        }

        [HttpGet]
        public ActionResult Recover()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult Recover(string email)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var token = _authService.GenerateResetPasswordToken(email);
                    var tokenString = Base64Helper.Base64UrlEncode(token.Token);

                    var resetLink = String.Format("/account/resetpassword/{0}/{1}", token.Id, tokenString);
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
        public ActionResult ResetPassword(int? id, string token)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (!id.HasValue || String.IsNullOrWhiteSpace(token))
                throw new HttpException(404, "Requested url is not valid");

            try
            {
                var isTokenValid = _authService.IsTokenValid(id.Value, token);
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
        public ActionResult ResetPassword(int id, string token, ResetPasswordModel resetPasswordModel)
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

        public ActionResult OAuth(string id, bool? isMobile)
        {
            if (isMobile.HasValue && isMobile.Value == true)
                Session[IS_MOBILE_SESSION_KEY] = true;

            return new OAuthLoginResult(id, "/account/oauthcallback");
        }

        public ActionResult OAuthCallback()
        {
            var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
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

            var oauthAccount = new LinkedAccount
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

                _authService.AddOrUpdateOAuthAccount(oauthAccount);
                ShowAlert("You have successfully updated your facebook account.", AlertType.Success);
            }
            else
            {
                if (user != null)
                {
                    //user exists
                    _authService.AddOrUpdateOAuthAccount(oauthAccount);
                    FormsAuthentication.SetAuthCookie(user.Username, true);
                }
                else
                {
                    Session[IS_OAUTH_KEY] = true;
                    Session[OAUTH_PROVIDER_KEY] = provider;
                    Session[OAUTH_PROVIDER_USERID_KEY] = providerId;
                    Session[OAUTH_EMAIL_KEY] = email;
                    Session[OAUTH_ACCESS_TOKEN_KEY] = accessToken;
                    
                    return RedirectToAction("SelectAccountType");
                }
            }

            if (isMobile.HasValue && isMobile.Value)
            {
                Session.Remove(IS_MOBILE_SESSION_KEY);
                return HandleMobileClientOAuthCallback(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public ActionResult FindFriends()
        {
            var userId = GetCurrentUserId();
            
            //Getting user access token
            var fbAccessToken = UserService.GetAccessToken(userId, OAuthProvider.Facebook);
            if (String.IsNullOrEmpty(fbAccessToken))
                return RedirectToAction("Index", "Home");

            //Getting facebook friends that also have account on zazz
            var fbFriendIds = _facebookService.FindZazzFbFriends(fbAccessToken)
                                            .Select(f => f.Id)
                                            .ToList();

            if (fbFriendIds.Count == 0)
                return RedirectToAction("Index", "Home");

            //Getting list of users that current user follows
            var currentFollows = _followService.GetFollows(userId)
                                               .Select(f => f.ToUserId);

            //Removing the users that current user follows from facebook friends
            fbFriendIds.RemoveAll(currentFollows.Contains);
            if (fbFriendIds.Count == 0)
                return RedirectToAction("Index", "Home");

            var vm = new FindFriendsViewModel
                     {
                         Friends = fbFriendIds.Select(id => new FriendViewModel
                                                            {
                                                                UserId = id,
                                                                Name = UserService.GetUserDisplayName(id),
                                                                Photo = PhotoService.GetUserDisplayPhoto(id)
                                                            })
                     };

            return View(vm);
        }

        public ActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
                FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
