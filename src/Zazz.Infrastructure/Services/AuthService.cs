using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;
        private readonly IFacebookService _facebookService;

        private const int PASS_MAX_LENGTH = 20;

        private const int TOKEN_SIZE = 512;

        public AuthService(IUoW uow, ICryptoService cryptoService,IFacebookService facebookService)
        {
            _uow = uow;
            _cryptoService = cryptoService;
            _facebookService = facebookService;
        }

        public void Login(string username, string password)
        {
            var user = _uow.UserRepository.GetByUsername(username);
            if (user == null)
                throw new NotFoundException();

            var passwordHash = _cryptoService.GeneratePasswordHash(password);
            if (passwordHash != user.Password)
                throw new InvalidPasswordException();

            user.LastActivity = DateTime.UtcNow;

            _uow.SaveChanges();
        }

        public User Register(User user, string password, bool createToken)
        {
            const string EMAIL_PATTERN = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                                         + "@"
                                         + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";

            var regex = new Regex(EMAIL_PATTERN);
            if (!regex.IsMatch(user.Email))
                throw new InvalidEmailException();

            
            if (password.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var usernameExists = _uow.UserRepository.ExistsByUsername(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = _uow.UserRepository.ExistsByEmail(user.Email);
            if (emailExists)
                throw new EmailExistsException();

            if (user.UserDetail.Birthdate != null && user.UserDetail.Birthdate.GetValueOrDefault().Year < 1900)
                throw new InvalidBirthdateException();

            if (user.UserDetail.UserType != null && !Enum.IsDefined(typeof(UserType), user.UserDetail.UserType))
                throw new InvalidUserType();

            if (user.UserDetail.UserType == UserType.User && user.UserDetail.PromoterType != null)
                throw new InvalidUserWithPromoterType();

            if (user.UserDetail.PromoterType != null && !Enum.IsDefined(typeof(PromoterType), user.UserDetail.PromoterType))
                throw new InvalidPromoterType();

            if (user.UserDetail.UserType == UserType.Promoter && user.UserDetail.MajorId != null)
                throw new InvalidPromoterWithMajorId();


            if (user.UserDetail.UserType == UserType.User && user.UserDetail.MajorId != null)
            {
                if (_uow.MajorRepository.GetById(user.UserDetail.MajorId.GetValueOrDefault()) == null)
                {
                    throw new InvalidMajorId();
                }
            }



            user.Password = _cryptoService.GeneratePasswordHash(password);

            if (createToken)
            {
                user.UserValidationToken = new UserValidationToken
                                           {
                                               ExpirationTime = DateTime.UtcNow.AddYears(1),
                                               Token = _cryptoService.GenerateKey(TOKEN_SIZE)
                                           };
            }

            _uow.UserRepository.InsertGraph(user);
            _uow.SaveChanges();

            return user;
        }

        public UserValidationToken GenerateResetPasswordToken(string email)
        {
            var userId = _uow.UserRepository.GetIdByEmail(email);
            if (userId == 0)
                throw new NotFoundException();

            var tokenExists = true;

            var token = _uow.ValidationTokenRepository.GetById(userId);
            if (token == null)
            {
                tokenExists = false;
                token = new UserValidationToken { Id = userId };
            }
                

            token.ExpirationTime = DateTime.UtcNow.AddDays(1);
            token.Token = _cryptoService.GenerateKey(TOKEN_SIZE);

            if (!tokenExists)
                _uow.ValidationTokenRepository.InsertGraph(token);

            _uow.SaveChanges();

            return token;
        }


        public UserValidationToken GenerateUserValidationToken(string email)
        {
            var userId = _uow.UserRepository.GetIdByEmail(email);
            if (userId == 0)
                throw new NotFoundException();

            var tokenExists = true;

            var token = _uow.ValidationTokenRepository.GetById(userId);
            if (token == null)
            {
                tokenExists = false;
                token = new UserValidationToken { Id = userId };
            }
                

            token.ExpirationTime = DateTime.UtcNow.AddYears(1);
            token.Token = _cryptoService.GenerateKey(TOKEN_SIZE);

            if (!tokenExists)
                _uow.ValidationTokenRepository.InsertGraph(token);

            _uow.SaveChanges();

            return token;
        }
        

        public bool IsTokenValid(int userId, string token)
        {
            var userToken = _uow.ValidationTokenRepository.GetById(userId);
            return IsTokenValid(userToken, token);
        }
        

        public bool IsTokenValidForUser(int userId, string token)
        {
            var userToken = _uow.ValidationTokenRepository.GetById(userId);
            return IsTokenValidForUser(userToken, token);
        }

        private bool IsTokenValidForUser(UserValidationToken userToken, string providedToken)
        {
            if (userToken == null)
                return false;

            var base64Token = Base64Helper.Base64UrlEncode(userToken.Token);
            return providedToken.Equals(base64Token);
        }



        private bool IsTokenValid(UserValidationToken userToken, string providedToken)
        {
            if (userToken == null)
                return false;

            if (userToken.ExpirationTime < DateTime.UtcNow)
                throw new TokenExpiredException();

            var base64Token = Base64Helper.Base64UrlEncode(userToken.Token);
            return providedToken.Equals(base64Token);
        }

        public void ResetPassword(int userId, string token, string newPassword)
        {
            if (newPassword.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var user = _uow.UserRepository.GetById(userId);
            var isTokenValid = IsTokenValid(user.UserValidationToken, token);
            if (!isTokenValid)
                throw new InvalidTokenException();

            user.Password = _cryptoService.GeneratePasswordHash(newPassword);

            _uow.ValidationTokenRepository.Remove(user.Id);
            _uow.SaveChanges();
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (newPassword.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var user = _uow.UserRepository.GetById(userId);
            if (user == null)
                throw new NotFoundException();

            var currentPasswordHash = _cryptoService.GeneratePasswordHash(currentPassword);
            if (currentPasswordHash != user.Password)
                throw new InvalidPasswordException();

            user.Password = _cryptoService.GeneratePasswordHash(newPassword);

            _uow.SaveChanges();
        }

        public User GetOAuthUser(LinkedAccount linkedAccount, string email)
        {
            var existingOAuthAccount = _uow.LinkedAccountRepository.GetOAuthAccountByProviderId(linkedAccount.ProviderUserId,
                                                                        linkedAccount.Provider);
            if (existingOAuthAccount != null)
                return existingOAuthAccount.User; // user and OAuth account exist

            var user = _uow.UserRepository.GetByEmail(email);

            return user;
        }

        public void AddOrUpdateOAuthAccount(LinkedAccount oauthAccount)
        {
            var account = _uow.LinkedAccountRepository.GetOAuthAccountByProviderId(oauthAccount.ProviderUserId,
                                                                                oauthAccount.Provider);

            if (account == null)
            {
                _uow.LinkedAccountRepository.InsertGraph(oauthAccount);
            }
            else
            {
                account.AccessToken = oauthAccount.AccessToken;

                if (account.User.AccountType == AccountType.Club)
                    _facebookService.UpdatePagesAccessToken(account.UserId, oauthAccount.AccessToken);
            }

            _uow.SaveChanges();
        }
    }
}