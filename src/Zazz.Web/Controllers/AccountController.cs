using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Mail;
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
using Zazz.Core.Models.Facebook;
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
        private readonly IFacebookService _facebookService;
        private readonly IFollowService _followService;
        private readonly IUoW _uow;
        private readonly IOAuthService _oAuthService;
        private readonly IFacebookHelper _facebookHelper;

        private const string IS_MOBILE_SESSION_KEY = "IsMobile";
        private const string IS_OAUTH_KEY = "IsOAuth";
        private const string IS_CLUB_KEY = "IsClub";
        private const string OAUTH_PROVIDER_KEY = "OAUTH_Provider";
        private const string OAUTH_FULLNAME_KEY = "OAUTH_FullName";
        private const string OAUTH_PROVIDER_USERID_KEY = "OAUTH_ProviderUserId";
        private const string OAUTH_EMAIL_KEY = "OAUTH_Email";
        private const string OAUTH_ACCESS_TOKEN_KEY = "OAUTH_AccessToken";
        private const string OAUTH_GENDER_KEY = "OAUTH_Gender";
        private const string OAUTH_PROFILE_PIC_KEY = "OAUTH_ProfilePic";
        private const string OAUTH_COVER_PIC_KEY = "OAUTH_CoverPic";


        public AccountController(
            IStaticDataRepository staticData,
            IAuthService authService,
            IUserService userService,
            IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper,
            IFacebookService facebookService,
            IFollowService followService,
            IUoW uow,
            IOAuthService oAuthService,
            IFacebookHelper facebookHelper,
            ICategoryService categoryService
            ) : base(userService, photoService, defaultImageHelper, staticData, categoryService)
        {
            _authService = authService;
            _facebookService = facebookService;
            _followService = followService;
            _uow = uow;
            _oAuthService = oAuthService;
            _facebookHelper = facebookHelper;
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
                    ModelState.AddModelError("", "Invalid login information");
                }
                catch (NotFoundException)
                {
                    ModelState.AddModelError("", "Invalid login information");
                }
            }

            return View(login);
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
            var vm = new RegisterUserViewModel();

            if (Session[IS_OAUTH_KEY] != null && ((bool)Session[IS_OAUTH_KEY]))
            {
                vm.IsOAuth = true;
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public async Task<ActionResult> RegisterUser(RegisterUserViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                           {
                               Username = vm.UserName,
                               AccountType = AccountType.User,
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
                                   IsPromoter = (vm.UserType == UserType.Promoter)
                               }
                           };

                var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
                var isOAuth = (bool?)Session[IS_OAUTH_KEY];
                string profilePhotoUrl = null;

                if (isOAuth.HasValue && isOAuth.Value)
                {
                    try
                    {
                        var provider = (OAuthProvider)Session[OAUTH_PROVIDER_KEY];
                        var providerUserId = Int64.Parse((string)Session[OAUTH_PROVIDER_USERID_KEY]);
                        var email = (string)Session[OAUTH_EMAIL_KEY];
                        var accessToken = (string)Session[OAUTH_ACCESS_TOKEN_KEY];
                        profilePhotoUrl = (string)Session[OAUTH_PROFILE_PIC_KEY];

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
                        ViewBag.IsMobile = (isMobile.HasValue && isMobile.Value);
                        return RedirectToAction("SessionExpired");
                    }
                    finally
                    {
                        ReleaseOAuthSessionValues();
                    }
                }

                try
                {
                    _authService.Register(user, vm.Password, !user.IsConfirmed);

                    var message = "";

                    try
                    {

                        var token = _authService.GenerateResetPasswordToken(user.Email);
                        var userId = _uow.UserRepository.GetIdByEmail(user.Email);
                        User _userDetails = UserService.GetUser(userId, true);
                        string userName = _userDetails.Username;

                        var tokenString = Base64Helper.Base64UrlEncode(token.Token);

                        var resetLink = String.Format("/account/ConfirmEmail/{0}/{1}", token.Id, tokenString);
                        message = String.Format(
                            "A conformation email has been sent to {0}. Please check your inbox.{1}", user.Email,
                            Environment.NewLine);
                        string body = "Dear " + UserService.GetUserDisplayName(userId) + " \n\n";
                        body += "Please click the link below to confirm your Account:\n";
                        body += "<a href='" + resetLink + "'>" + resetLink + "</a>\n\nTeam Zazz";

                        MailMessage mail = new MailMessage("team@zazzlife.com", user.Email, "Zazzlife - Account Conformation", body);
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient("smtp.office365.com", 587);
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("team@zazzlife.com", "Zazzxsw21q");// Enter seders User name and password  
                        smtp.EnableSsl = true;
                        smtp.ServicePoint.MaxIdleTime = 1;
                        //smtp.Send(mail);
                        ShowAlert(message + " => " + resetLink, AlertType.Success);



                    }
                    catch (Exception)
                    {
                        ShowAlert("Error for sending email. Pl. Try again", AlertType.Warning);
                    }


                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    await TryUpdateUserPic(user, profilePhotoUrl, null);

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
                        //return RedirectToAction("Index", "Home");
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

        [HttpGet]
        public ActionResult RegisterClub()
        {
            var vm = new RegisterClubViewModel();

            if (Session[IS_OAUTH_KEY] != null && ((bool)Session[IS_OAUTH_KEY]))
            {
                vm.IsOAuth = true;
            }

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public async Task<ActionResult> RegisterClub(RegisterClubViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = vm.UserName,
                    AccountType = AccountType.Club,
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
                    ClubDetail = new ClubDetail()
                };

                var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
                var isOAuth = (bool?)Session[IS_OAUTH_KEY];

                string profilePhotoUrl = null;
                string coverPhotoUrl = null;

                if (isOAuth.HasValue && isOAuth.Value)
                {
                    try
                    {
                        var provider = (OAuthProvider)Session[OAUTH_PROVIDER_KEY];
                        var providerUserId = Int64.Parse((string)Session[OAUTH_PROVIDER_USERID_KEY]);
                        var email = (string)Session[OAUTH_EMAIL_KEY];
                        var accessToken = (string)Session[OAUTH_ACCESS_TOKEN_KEY];
                        profilePhotoUrl = (string)Session[OAUTH_PROFILE_PIC_KEY];
                        coverPhotoUrl = (string)Session[OAUTH_COVER_PIC_KEY];

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
                        ViewBag.IsMobile = (isMobile.HasValue && isMobile.Value);
                        return RedirectToAction("SessionExpired");
                    }
                    finally
                    {
                        ReleaseOAuthSessionValues();
                    }
                }

                try
                {
                    _authService.Register(user, vm.Password, !user.IsConfirmed);

                    


                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    await TryUpdateUserPic(user, profilePhotoUrl, coverPhotoUrl);

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

        private async Task TryUpdateUserPic(User user, string profilePhotoUrl, string coverPhotoUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(profilePhotoUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var photoData = await client.GetByteArrayAsync(profilePhotoUrl);

                        var photo = new Photo
                                    {
                                        UploadDate = DateTime.UtcNow,
                                        UserId = user.Id
                                    };

                        using (var ms = new MemoryStream(photoData))
                        {
                            var photoId = PhotoService.SavePhoto(photo, ms, false, null);
                            UserService.ChangeProfilePic(user.Id, photoId);
                        }
                    }
                }

                if (user.AccountType == AccountType.Club && !String.IsNullOrEmpty(coverPhotoUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var photoData = await client.GetByteArrayAsync(coverPhotoUrl);

                        var photo = new Photo
                        {
                            UploadDate = DateTime.UtcNow,
                            UserId = user.Id
                        };

                        using (var ms = new MemoryStream(photoData))
                        {
                            var photoId = PhotoService.SavePhoto(photo, ms, false, null);
                            UserService.ChangeCoverPic(user.Id, photoId);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ReleaseOAuthSessionValues()
        {
            Session.Remove(IS_MOBILE_SESSION_KEY);
            Session.Remove(IS_OAUTH_KEY);
            Session.Remove(IS_CLUB_KEY);
            Session.Remove(OAUTH_PROVIDER_KEY);
            Session.Remove(OAUTH_FULLNAME_KEY);
            Session.Remove(OAUTH_PROVIDER_USERID_KEY);
            Session.Remove(OAUTH_EMAIL_KEY);
            Session.Remove(OAUTH_ACCESS_TOKEN_KEY);
            Session.Remove(OAUTH_GENDER_KEY);
            Session.Remove(OAUTH_PROFILE_PIC_KEY);
            Session.Remove(OAUTH_COVER_PIC_KEY);
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
        public ActionResult ConfirmEmail(int? id, string token)
        {
            if (!id.HasValue || String.IsNullOrWhiteSpace(token))
                throw new HttpException(404, "Requested url is not valid");

            try
            {
                var isTokenValid = _authService.IsTokenValidForUser(id.Value, token);
                if (!isTokenValid)
                    throw new HttpException(404, "Requested url is not valid");


                User confirmUser = _uow.UserRepository.GetById(id.Value);
                confirmUser.IsConfirmed = true;
                //_uow.UserRepository.Update(confirmUser);
                _uow.SaveChanges();
                ShowAlert("Your email successfully confirm.",AlertType.Success);
                //return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                //ShowAlert("This token has been expired. Please request a new one.", AlertType.Warning);
                //return RedirectToAction("Recover");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Recover()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View(new RecoverAccountViewModel());
        }

        [HttpGet]
        public ActionResult RecoverUser()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View(new RecoverUserViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult RecoverUser(RecoverUserViewModel vm)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var token = _authService.GenerateResetPasswordToken(vm.Email);
                    var userId = _uow.UserRepository.GetIdByEmail(vm.Email);
                    User _userDetails = UserService.GetUser(userId, true);
                    string userName = _userDetails.Username;


                    var message = String.Format(
                        "Your Username has been sent to {0}. Please check your inbox.{1}", vm.Email,
                        Environment.NewLine);
                    string body = "Dear " + UserService.GetUserDisplayName(userId) + " \n\n";
                    body += "Your Username is : " + userName + ".\n\nTeam Zazz";

                    MailMessage mail = new MailMessage("team@zazzlife.com", vm.Email, "Zazzlife - Username", body);
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient("smtp.office365.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("team@zazzlife.com", "Zazzxsw21q");// Enter seders User name and password  
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    ShowAlert(message, AlertType.Success);
                }
                catch (NotFoundException)
                {
                    ShowAlert("Email not found", AlertType.Warning);
                }
            }

            return View(vm);
        }



        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult Recover(RecoverAccountViewModel vm)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                try
                {
                    var token = _authService.GenerateResetPasswordToken(vm.Email);
                    var userId = _uow.UserRepository.GetIdByEmail(vm.Email);
                    User _userDetails = UserService.GetUser(userId, true);
                    string userName = _userDetails.Username;

                    var tokenString = Base64Helper.Base64UrlEncode(token.Token);
                    
                    var resetLink = String.Format("/account/resetpassword/{0}/{1}", token.Id, tokenString);
                    var message = String.Format(
                        "A recovery email has been sent to {0}. Please check your inbox.{1}", vm.Email,
                        Environment.NewLine);
                    string body = "Dear "+UserService.GetUserDisplayName(userId)+" \n\n";
                    body += "Your request to reset the password for your account has been processed.\n\n";
                    body += "Please click the link below to reset your password:\n";
                    body += "<a href='" + resetLink + "'>Reset Password</a>\n\nTeam Zazz";

                    MailMessage mail = new MailMessage("team@zazzlife.com", vm.Email, "Zazzlife - Password Reset", body);
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient("smtp.office365.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("team@zazzlife.com", "Zazzxsw21q");// Enter seders User name and password  
                    smtp.EnableSsl = true;
                    smtp.ServicePoint.MaxIdleTime = 1;
                    smtp.Send(mail);
                    ShowAlert(message, AlertType.Success);

                }
                catch (NotFoundException)
                {
                    ShowAlert("Email not found", AlertType.Warning);
                }
            }

            return View(vm);
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

        public ActionResult OAuth(string id, bool? isMobile, bool? isClub)
        {
            if (isMobile.HasValue && isMobile.Value == true)
                Session[IS_MOBILE_SESSION_KEY] = true;
            if (isClub.HasValue && isClub.Value == true)
                Session[IS_CLUB_KEY] = true;
            else
                Session[IS_CLUB_KEY] = false;

            return new OAuthLoginResult(id, "/account/oauthcallback");
        }

        public ActionResult OAuthCallback()
        {
            var isMobile = (bool?)Session[IS_MOBILE_SESSION_KEY];
            var isClub = (bool?)Session[IS_CLUB_KEY];
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
                    //user does not have an account (there is a check if it's a mobile client on Post register pages)

                    //getting some basic user info for later use.
                    var fbBasicUserInfo = new FbBasicUserInfo
                                          {
                                              Gender = Gender.NotSpecified,
                                              CoverPicUrl = null,
                                              ProfilePicUrl = null
                                          };

                    try
                    {
                        fbBasicUserInfo = _facebookHelper.GetBasicUserInfo(accessToken);
                    }
                    catch (Exception)
                    { }

                    Session[IS_OAUTH_KEY] = true;
                    Session[OAUTH_PROVIDER_KEY] = provider;
                    Session[OAUTH_FULLNAME_KEY] = name;
                    Session[OAUTH_PROVIDER_USERID_KEY] = providerId;
                    Session[OAUTH_EMAIL_KEY] = email;
                    Session[OAUTH_ACCESS_TOKEN_KEY] = accessToken;
                    Session[OAUTH_GENDER_KEY] = fbBasicUserInfo.Gender;
                    Session[OAUTH_PROFILE_PIC_KEY] = fbBasicUserInfo.ProfilePicUrl;
                    Session[OAUTH_COVER_PIC_KEY] = fbBasicUserInfo.CoverPicUrl;

                    if (isClub.HasValue && isClub.Value == true)
                        return RedirectToAction("RegisterClub");
                    else
                        return RedirectToAction("RegisterUser");
                }
            }


            //user had an account, checking if it's mobile to redirect to the right place
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
