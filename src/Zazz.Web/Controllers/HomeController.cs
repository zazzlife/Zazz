using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json;
using PoliteCaptcha;
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
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Facebook;
using Zazz.Data;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.OAuthAuthorizationServer;
using System.Net.Mail;

namespace Zazz.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly IUoW _uow;
        private readonly IAuthService _authService;
        private readonly IFacebookService _facebookService;
        private readonly IFollowService _followService;
        private readonly IOAuthService _oAuthService;
        private readonly IFacebookHelper _facebookHelper;
        private readonly IPhotoService _photoService;

        private const string IS_MOBILE_SESSION_KEY = "IsMobile";
        private const string IS_OAUTH_KEY = "IsOAuth";
        private const string OAUTH_PROVIDER_KEY = "OAUTH_Provider";
        private const string OAUTH_FULLNAME_KEY = "OAUTH_FullName";
        private const string OAUTH_PROVIDER_USERID_KEY = "OAUTH_ProviderUserId";
        private const string OAUTH_EMAIL_KEY = "OAUTH_Email";
        private const string OAUTH_ACCESS_TOKEN_KEY = "OAUTH_AccessToken";
        private const string OAUTH_GENDER_KEY = "OAUTH_Gender";
        private const string OAUTH_PROFILE_PIC_KEY = "OAUTH_ProfilePic";
        private const string OAUTH_COVER_PIC_KEY = "OAUTH_CoverPic";

        public HomeController(
            IPhotoService photoService,
            IUserService userService,
            IStaticDataRepository staticDataRepository,
            ICategoryService categoryService,
            IDefaultImageHelper defaultImageHelper,
            IFeedHelper feedHelper,
            IUoW uow,
            IStaticDataRepository staticData,
            IAuthService authService,
            IFacebookService facebookService,
            IFollowService followService,
            IOAuthService oAuthService,
            IFacebookHelper facebookHelper
            ) :
            base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _feedHelper = feedHelper;
            _uow = uow;
            _authService = authService;
            _facebookService = facebookService;
            _followService = followService;
            _oAuthService = oAuthService;
            _facebookHelper = facebookHelper;
            _photoService = photoService;
        }

        // TODO: rename
        public IEnumerable<TagStatViewModel> GetRealTagStats()
        {
            var stats = _uow.TagRepository.GetClubTagStats()
                .Select(t => new TagStatViewModel
                {
                    ClubId = t.ClubId,
                    ClubUsername = t.Club.Username,
                    Count = t.Count
                })
                .ToList();

            for (var i = 0; i < stats.Count; i++)
            {
                stats[i].Photo = _photoService.GetUserDisplayPhoto(stats[i].ClubId).VerySmallLink;
            }

            return stats;
        }

        public JsonNetResult IsAvailable(string username)
        {
            if (String.IsNullOrWhiteSpace(username) || username.Length < 2 || username.Length > 20)
                return new JsonNetResult(false);

            var usernameExists = _uow.UserRepository.ExistsByUsername(username);
            return usernameExists ? new JsonNetResult(false) : new JsonNetResult(true);
        }

        public ActionResult Index(bool userFollow = false, bool inSameCity = false)
        {
            ViewBag.userFollow = userFollow;
            ViewBag.inSameCity = inSameCity;

            if (User.Identity.IsAuthenticated)
            {
                 var userId = UserService.GetUserId(User.Identity.Name);
                 var user = UserService.GetUser(User.Identity.Name);
                
                 if (UserService.OAuthAccountExists(user.Id, OAuthProvider.Facebook))
                 {
                     if (user.AccountType == AccountType.Club)
                     {
                         var allPages = _facebookService.GetUserPages(userId);
                         var existingPageIds = _uow.FacebookPageRepository.GetUserPages(userId)
                                                   .Select(f => f.FacebookId)
                                                   .ToList();


                         foreach (var fbPage in allPages)
                         {
                             if (existingPageIds.Contains(fbPage.Id))
                             {
                                 try
                                 {
                                     _facebookService.SyncPageEvents(fbPage.Id);
                                     _facebookService.SyncPagePhotos(fbPage.Id);
                                     _facebookService.SyncPageStatuses(fbPage.Id);
                                 }
                                 catch(Exception)
                                 { }
                             }
                         }
                     }
                 }


                 foreach (var follows in user.Follows)
                 {
                     try
                     {
                        User usr = UserService.GetUser(follows.ToUserId);
                        if(usr.AccountType == AccountType.Club)
                        {
                            if (UserService.OAuthAccountExists(follows.ToUserId, OAuthProvider.Facebook))
                            {
                                var allPages = _facebookService.GetUserPages(follows.ToUserId);
                                var existingPageIds = _uow.FacebookPageRepository.GetUserPages(follows.ToUserId)
                                                          .Select(f => f.FacebookId)
                                                          .ToList();


                                foreach (var fbPage in allPages)
                                {
                                    if (existingPageIds.Contains(fbPage.Id))
                                    {
                                        try
                                        {
                                            _facebookService.SyncPageEvents(fbPage.Id);
                                            _facebookService.SyncPagePhotos(fbPage.Id);
                                            _facebookService.SyncPageStatuses(fbPage.Id);
                                        }
                                        catch (Exception)
                                        { }
                                    }
                                }
                            }
                        }
                     }
                     catch(Exception)
                     {
                     }
                 }


                var feeds = _feedHelper.GetFeeds(user.Id, 0, userFollow, inSameCity);
                
                var vm = new UserHomeViewModel
                         {
                             AccountType = user.AccountType,
                             Feeds = feeds,
                             HasFacebookAccount = UserService.OAuthAccountExists(user.Id, OAuthProvider.Facebook)
                         };





                return View("UserHome", vm);
            }
            else
            {
                var vm = new RegisterUserHomeViewModel
                {
                    Majors = StaticDataRepository.GetMajors(),
                    PromoterTypes = (IEnumerable<PromoterType>)Enum.GetValues(typeof(PromoterType))
                };

                if (Session[IS_OAUTH_KEY] != null && ((bool)Session[IS_OAUTH_KEY]))
                {
                    var email = (string)Session[OAUTH_EMAIL_KEY];
                    var gender = (Gender)Session[OAUTH_GENDER_KEY];

                    vm.IsOAuth = true;
                    vm.Email = email;
                    vm.Gender = gender;
                }
                return View("LandingPage", vm);
            }
        }

        private void ReleaseOAuthSessionValues()
        {
            System.Web.HttpContext.Current.Cache.Remove(IS_MOBILE_SESSION_KEY);
            Session.Remove(IS_OAUTH_KEY);
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

        [HttpPost, ValidateAntiForgeryToken, ValidateSpamPrevention]
        public ActionResult Register(RegisterUserHomeViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User {
                    Username = vm.UserName,
                    AccountType = AccountType.User,
                    Email = vm.Email,
                    IsConfirmed = false,
                    JoinedDate = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    Birth = vm.Birth,
                    Preferences = new UserPreferences {
                        SyncFbEvents = true,
                        SyncFbImages = false,
                        SyncFbPosts = false,
                        SendSyncErrorNotifications = true
                    },
                    UserDetail = new UserDetail {
                        Gender = vm.Gender
                    },
                    tagline = vm.TagLine
                };

                if (vm.UserType == UserType.User) {
                    user.UserDetail.IsPromoter = false;
                    user.UserDetail.MajorId = vm.MajorId;
                }
                else {
                    user.UserDetail.IsPromoter = true;
                    user.UserDetail.PromoterType = vm.PromoterType;
                }

                var isMobile = (bool?)System.Web.HttpContext.Current.Cache[IS_MOBILE_SESSION_KEY];
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

                        var token = _authService.GenerateUserValidationToken(user.Email);
                        var userId = _uow.UserRepository.GetIdByEmail(user.Email);
                        User _userDetails = UserService.GetUser(userId, true);
                        string userName = _userDetails.Username;

                        var tokenString = Base64Helper.Base64UrlEncode(token.Token);

                        Uri currentUri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
                        string pathQuery = currentUri.PathAndQuery;
                        string hostName = currentUri.ToString().Replace(pathQuery, "");

                        var resetLink = hostName + String.Format("/account/ConfirmEmail/{0}/{1}", token.Id, tokenString);
                        message = String.Format(
                            "A confirmation email has been sent to {0}. Please check your inbox.{1}", user.Email,
                            Environment.NewLine);
                        string body = "Dear " + UserService.GetUserDisplayName(userId) + " \n\n";
                        body += "Please click the link below to confirm your Account:\n";
                        body += "<a href='" + resetLink + "'>" + resetLink + "</a>\n\nTeam Zazz";

                        MailMessage mail = new MailMessage("team@zazzlife.com", user.Email, "Zazzlife - Account Conformation", body);
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient("smtp.office365.com", 587);
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("team@zazzlife.com", "Zazzq12wsx");// Enter seders User name and password  
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                        ShowAlert(message, AlertType.Info);
                    }
                    catch (Exception)
                    {
                        ShowAlert("Error for sending email.", AlertType.Warning);
                    }


                    FormsAuthentication.SetAuthCookie(user.Username, true);

                    if (isMobile.HasValue && isMobile.Value)
                    {
                        System.Web.HttpContext.Current.Cache.Remove(IS_MOBILE_SESSION_KEY);
                        return HandleMobileClientOAuthCallback(user);
                    }
                    else if (isOAuth.HasValue && isOAuth.Value)
                    {
                        return RedirectToAction("FindFriends");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { validemail = "true" });
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

            vm.Majors = StaticDataRepository.GetMajors();
            vm.PromoterTypes = (IEnumerable<PromoterType>)Enum.GetValues(typeof(PromoterType));
            
            return View("LandingPage", vm);
        }

        [Authorize]
        public ActionResult Categories(string @select, string @tag, bool userFollow = false, bool inSameCity = false)
        {
            var user = UserService.GetUser(User.Identity.Name);

            FeedsViewModel feeds;

            var availableCategories = StaticDataRepository.GetCategories().ToList();

            var selectedCategories = String.IsNullOrEmpty(@select)
                                    ? Enumerable.Empty<string>()
                                    : @select.Split(',');

            List<int> selectedTags = null;
            if(!String.IsNullOrEmpty(@tag))
            {
                var tags = @tag.Split(',');
                selectedTags = new List<int>();
                foreach (string s in tags)
                {
                    try
                    {
                        selectedTags.Add(int.Parse(s));
                    }
                    catch { }
                }
                if (selectedTags.Count == 0)
                {
                    selectedTags = null;
                }
            }

            if (!String.IsNullOrEmpty(@select) || !String.IsNullOrEmpty(@tag))
            {
                var selectedCategoriesId =
                    availableCategories.Where(t => selectedCategories.Contains(t.Name,
                                                                               StringComparer.InvariantCultureIgnoreCase))
                                       .Select(t => t.Id);

                feeds = _feedHelper.GetCategoryFeeds(user.Id, selectedCategoriesId.ToList(), 0, selectedTags, userFollow, inSameCity);
            }
            else
            {
                feeds = _feedHelper.GetFeeds(user.Id, 0, userFollow, inSameCity);
            }

            if (Request.IsAjaxRequest())
                return View("_FeedsPartial", feeds);

            var vm = new CategoriesPageViewModel
                     {
                         AvailableCategories = availableCategories.Select(t => t.Name),
                         SelectedCategories = selectedCategories,
                         Feeds = feeds,
                         AccountType = user.AccountType,
                         HasFacebookAccount = UserService.OAuthAccountExists(user.Id, OAuthProvider.Facebook)
                     };

            return View(vm);
        }

        [Authorize]
        public ActionResult LoadMoreFeeds(int lastFeedId, string @select, string @tag, bool showPhotos = true, bool userFollow = false, bool inSameCity = false)
        {
            var user = UserService.GetUser(User.Identity.Name);

            FeedsViewModel feeds;

            List<int> selectedTags = null;
            if(!String.IsNullOrEmpty(@tag))
            {
                var tags = @tag.Split(',');
                selectedTags = new List<int>();
                foreach (string s in tags)
                {
                    try
                    {
                        selectedTags.Add(int.Parse(s));
                    }
                    catch { }
                }
                if (selectedTags.Count == 0)
                {
                    selectedTags = null;
                }
            }

            if (!String.IsNullOrEmpty(@select) || selectedTags != null)
            {
                var availableCategories = StaticDataRepository.GetCategories().ToList();
                var selectedCategories = @select.Split(',');

                var selectedCategoriesId =
                    availableCategories.Where(t => selectedCategories.Contains(t.Name,
                                                                               StringComparer.InvariantCultureIgnoreCase))
                                       .Select(t => t.Id);

                feeds = _feedHelper.GetCategoryFeeds(user.Id, selectedCategoriesId.ToList(), lastFeedId, tags: selectedTags);
            }
            else
            {
                feeds = _feedHelper.GetFeeds(user.Id, lastFeedId, userFollow, inSameCity);
            }

            return View("_FeedsPartial", feeds);
        }

        [Authorize]
        public ActionResult Clubs(string type, string schoolName)
        {
            var userId = GetCurrentUserId();
            IQueryable<User> clubs = null;

            if (String.IsNullOrEmpty(type) || type.Equals("clubs", StringComparison.InvariantCultureIgnoreCase))
            {
                clubs = _uow.FollowRepository.GetClubsThatUserDoesNotFollow(userId);
            }
            else if (type.Equals("myclubs", StringComparison.InvariantCultureIgnoreCase))
            {
                clubs = _uow.FollowRepository.GetClubsThatUserFollows(userId);
            }
            else if (type.Equals("schoolclubs", StringComparison.InvariantCultureIgnoreCase))
            {
                int? schoolId = null;

                if (!String.IsNullOrWhiteSpace(schoolName))
                {
                    var school = StaticDataRepository.GetSchools()
                            .FirstOrDefault(s => s.Name.Equals(schoolName, StringComparison.InvariantCultureIgnoreCase));

                    if (school != null)
                        schoolId = school.Id;
                }

                if (schoolId.HasValue)
                {
                    clubs = _uow.UserRepository.GetSchoolClubs(schoolId.Value);
                }
                else
                {
                    clubs = _uow.UserRepository.GetSchoolClubs();
                }
            }

            var vm = new List<ClubViewModel>();
            if (clubs != null)
            {
                var items = clubs.Select(x => new
                {
                    x.Id,
                    ProfileImageId = x.ProfilePhotoId,
                    CoverImageId = x.ClubDetail.CoverPhotoId,
                    IsFollowing = x.Followers.Any(f => f.FromUserId == userId),
                    ClubTypes = x.ClubDetail.ClubTypes
                }).ToList();

                vm.AddRange(items.Select(x => new ClubViewModel
                {
                    ClubId = x.Id,
                    ClubName = UserService.GetUserDisplayName(x.Id),
                    ProfileImageLink = x.ProfileImageId.HasValue
                        ? PhotoService.GeneratePhotoUrl(x.Id, x.ProfileImageId.Value)
                        : DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified),
                    CoverImageLink = x.CoverImageId.HasValue
                        ? PhotoService.GeneratePhotoUrl(x.Id, x.CoverImageId.Value)
                        : DefaultImageHelper.GetDefaultCoverImage(),
                    IsCurrentUserFollowing = x.IsFollowing,
                    CurrentUserId = userId,
                    clubtypes = x.ClubTypes
                }));
            }

            return Request.IsAjaxRequest() ? View("_ClubsList", vm) : View(vm);
        }

        public string GetAllCategories()
        {
            var categories = StaticDataRepository.GetCategories().Select(c => c.Name);
            return JsonConvert.SerializeObject(categories, Formatting.None);
        }

        [Authorize]
        public JsonNetResult Search(string q)
        {
            var users = UserService.Search(q);
            var response = users.Select(u => new AutocompleteResponse
                                             {
                                                 Id = u.UserId,
                                                 Value = u.DisplayName,
                                                 Img = u.DisplayPhoto.VerySmallLink
                                             });

            return new JsonNetResult(response);
        }

        public class AutocompleteResponse
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public string Img { get; set; }
        }
    }

    public class LoginSuccessController : Controller
    {
        public string Index()
        {
            return String.Empty;
        }
    }


}
