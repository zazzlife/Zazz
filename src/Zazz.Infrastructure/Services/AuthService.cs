using System;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;
        private readonly ICacheService _cacheService;

        private const int PASS_MAX_LENGTH = 20;

        public AuthService(IUoW uow, ICryptoService cryptoService, ICacheService cacheService)
        {
            _uow = uow;
            _cryptoService = cryptoService;
            _cacheService = cacheService;
        }

        public void Login(string username, string password)
        {
            var user = _uow.UserRepository.GetByUsername(username);
            if (user == null)
                throw new NotFoundException();

            var decryptedPassword = _cryptoService.DecryptPassword(user.Password, user.PasswordIV);
            if (decryptedPassword != password)
                throw new InvalidPasswordException();

            user.LastActivity = DateTime.UtcNow;

            _uow.SaveChanges();
        }

        public User Register(User user, string password, bool createToken)
        {
            if (password.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var usernameExists = _uow.UserRepository.ExistsByUsername(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = _uow.UserRepository.ExistsByEmail(user.Email);
            if (emailExists)
                throw new EmailExistsException();

            var iv = String.Empty;
            user.Password = _cryptoService.EncryptPassword(password, out iv);
            user.PasswordIV = Convert.FromBase64String(iv);

            if (createToken)
            {
                user.UserValidationToken = new UserValidationToken
                                           {
                                               ExpirationTime = DateTime.UtcNow.AddYears(1),
                                               Token = Guid.NewGuid()
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
            token.Token = Guid.NewGuid();

            if (!tokenExists)
                _uow.ValidationTokenRepository.InsertGraph(token);

            _uow.SaveChanges();

            return token;
        }

        public bool IsTokenValid(int userId, Guid token)
        {
            var userToken = _uow.ValidationTokenRepository.GetById(userId);
            return IsTokenValid(userToken, token);
        }

        private bool IsTokenValid(UserValidationToken userToken, Guid providedToken)
        {
            if (userToken == null)
                return false;

            if (userToken.ExpirationTime < DateTime.UtcNow)
                throw new TokenExpiredException();

            return providedToken.Equals(userToken.Token);
        }

        public void ResetPassword(int userId, Guid token, string newPassword)
        {
            if (newPassword.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var user = _uow.UserRepository.GetById(userId);
            var isTokenValid = IsTokenValid(user.UserValidationToken, token);
            if (!isTokenValid)
                throw new InvalidTokenException();

            string iv;
            user.Password = _cryptoService.EncryptPassword(newPassword, out iv);
            user.PasswordIV = Convert.FromBase64String(iv);

            _uow.ValidationTokenRepository.Remove(user.Id);
            _uow.SaveChanges();

            _cacheService.RemovePassword(userId);
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (newPassword.Length > PASS_MAX_LENGTH)
                throw new PasswordTooLongException();

            var user = _uow.UserRepository.GetById(userId);

            var decryptedPassword = _cryptoService.DecryptPassword(user.Password, user.PasswordIV);
            if (currentPassword != decryptedPassword)
                throw new InvalidPasswordException();

            string iv;
            user.Password = _cryptoService.EncryptPassword(newPassword, out iv);
            user.PasswordIV = Convert.FromBase64String(iv);

            _uow.SaveChanges();
        }

        public User GetOAuthUser(OAuthAccount oAuthAccount, string email)
        {
            var existingOAuthAccount = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(oAuthAccount.ProviderUserId,
                                                                        oAuthAccount.Provider);
            if (existingOAuthAccount != null)
                return existingOAuthAccount.User; // user and OAuth account exist

            var user = _uow.UserRepository.GetByEmail(email);

            return null;
        }

        public void AddOrUpdateOAuthAccount(OAuthAccount oauthAccount)
        {
            var account = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(oauthAccount.ProviderUserId,
                                                                                oauthAccount.Provider);

            if (account == null)
            {
                _uow.OAuthAccountRepository.InsertGraph(oauthAccount);
                
            }
            else
            {
                account.AccessToken = oauthAccount.AccessToken;
            }

            _uow.SaveChanges();
        }
    }
}