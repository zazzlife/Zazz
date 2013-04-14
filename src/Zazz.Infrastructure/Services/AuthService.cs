using System;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUoW _uow;
        private readonly ICryptoService _cryptoService;

        public AuthService(IUoW uow, ICryptoService cryptoService)
        {
            _uow = uow;
            _cryptoService = cryptoService;
        }

        public void Login(string username, string password)
        {
            var passwordHash = _cryptoService.GeneratePasswordHash(password);
            var user = _uow.UserRepository.GetByUsername(username);

            if (user == null)
                throw new UserNotExistsException();

            if (passwordHash != user.Password)
                throw new InvalidPasswordException();

            user.LastActivity = DateTime.UtcNow;

            _uow.SaveChanges();
        }

        public User Register(User user, bool createToken)
        {
            if (user.UserDetail == null)
                throw new ArgumentNullException();

            var usernameExists = _uow.UserRepository.ExistsByUsername(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = _uow.UserRepository.ExistsByEmail(user.Email);
            if (emailExists)
                throw new EmailExistsException();

            var hashedPassword = _cryptoService.GeneratePasswordHash(user.Password);
            user.Password = hashedPassword;

            user.UserDetail.JoinedDate = DateTime.UtcNow;

            if (createToken)
            {
                user.ValidationToken = new ValidationToken
                                           {
                                               ExpirationDate = DateTime.UtcNow.AddYears(1),
                                               Token = Guid.NewGuid()
                                           };
            }

            _uow.UserRepository.InsertGraph(user);

            _uow.SaveChanges();

            return user;
        }

        public ValidationToken GenerateResetPasswordToken(string email)
        {
            var userId = _uow.UserRepository.GetIdByEmail(email);
            if (userId == 0)
                throw new EmailNotExistsException();

            var token = new ValidationToken
                            {
                                ExpirationDate = DateTime.UtcNow.AddDays(1),
                                Id = userId,
                                Token = Guid.NewGuid(),
                            };

            var oldToken = _uow.ValidationTokenRepository.GetById(userId);
            if (oldToken != null)
                _uow.ValidationTokenRepository.Remove(oldToken);

            _uow.ValidationTokenRepository.InsertGraph(token);
            _uow.SaveChanges();

            return token;
        }

        public bool IsTokenValid(int userId, Guid token)
        {
            var userToken = _uow.ValidationTokenRepository.GetById(userId);
            if (userToken == null)
                return false;

            if (userToken.ExpirationDate < DateTime.UtcNow)
                throw new TokenExpiredException();

            return token.Equals(userToken.Token);
        }

        public void ResetPassword(int userId, Guid token, string newPassword)
        {
            var isTokenValid = IsTokenValid(userId, token);
            if (!isTokenValid)
                throw new InvalidTokenException();

            var user = _uow.UserRepository.GetById(userId);
            var newPassHash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPassHash;
            _uow.ValidationTokenRepository.Remove(user.Id);
            _uow.SaveChanges();
        }

        public void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _uow.UserRepository.GetById(userId);

            var currentPassHash = _cryptoService.GeneratePasswordHash(currentPassword);
            if (currentPassHash != user.Password)
                throw new InvalidPasswordException();

            var newPasshash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPasshash;

            _uow.SaveChanges();
        }

        public User GetOAuthUser(OAuthAccount oAuthAccount, string email)
        {
            var existingOAuthAccount = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(oAuthAccount.ProviderUserId,
                                                                        oAuthAccount.Provider);
            if (existingOAuthAccount != null)
                return existingOAuthAccount.User; // user and OAuth account exist

            var user = _uow.UserRepository.GetByEmail(email);
            if (user != null)
            {
                oAuthAccount.UserId = user.Id;
                _uow.OAuthAccountRepository.InsertGraph(oAuthAccount);

                _uow.SaveChanges();

                return user;
            }

            return null;
        }

        public void AddOAuthAccount(OAuthAccount oauthAccount)
        {
            var check = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(oauthAccount.ProviderUserId,
                                                                                oauthAccount.Provider);

            if (check == null)
            {
                _uow.OAuthAccountRepository.InsertGraph(oauthAccount);
                _uow.SaveChanges();
            }
        }
    }
}