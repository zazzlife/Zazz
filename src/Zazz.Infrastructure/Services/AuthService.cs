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

        public async Task LoginAsync(string username, string password)
        {
            var passwordHash = _cryptoService.GeneratePasswordHash(password);
            var user = await _uow.UserRepository.GetByUsernameAsync(username);

            if (user == null)
                throw new UserNotExistsException();

            if (passwordHash != user.Password)
                throw new InvalidPasswordException();

            user.LastActivity = DateTime.UtcNow;

            _uow.SaveChanges();
        }

        public async Task<User> RegisterAsync(User user, bool createToken)
        {
            if (user.UserDetail == null)
                throw new ArgumentNullException();

            var usernameExists = await _uow.UserRepository.ExistsByUsernameAsync(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = await _uow.UserRepository.ExistsByEmailAsync(user.Email);
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

        public async Task<ValidationToken> GenerateResetPasswordTokenAsync(string email)
        {
            var userId = await _uow.UserRepository.GetIdByEmailAsync(email);
            if (userId == 0)
                throw new EmailNotExistsException();

            var token = new ValidationToken
                            {
                                ExpirationDate = DateTime.UtcNow.AddDays(1),
                                Id = userId,
                                Token = Guid.NewGuid(),
                            };

            var oldToken = await _uow.ValidationTokenRepository.GetByIdAsync(userId);
            if (oldToken != null)
                _uow.ValidationTokenRepository.Remove(oldToken);

            _uow.ValidationTokenRepository.InsertGraph(token);
            _uow.SaveChanges();

            return token;
        }

        public async Task<bool> IsTokenValidAsync(int userId, Guid token)
        {
            var userToken = await _uow.ValidationTokenRepository.GetByIdAsync(userId);
            if (userToken == null)
                return false;

            if (userToken.ExpirationDate < DateTime.UtcNow)
                throw new TokenExpiredException();

            return token.Equals(userToken.Token);
        }

        public async Task ResetPasswordAsync(int userId, Guid token, string newPassword)
        {
            var isTokenValid = await IsTokenValidAsync(userId, token);
            if (!isTokenValid)
                throw new InvalidTokenException();

            var user = await _uow.UserRepository.GetByIdAsync(userId);
            var newPassHash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPassHash;
            await _uow.ValidationTokenRepository.RemoveAsync(user.Id);
            _uow.SaveChanges();
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _uow.UserRepository.GetByIdAsync(userId);

            var currentPassHash = _cryptoService.GeneratePasswordHash(currentPassword);
            if (currentPassHash != user.Password)
                throw new InvalidPasswordException();

            var newPasshash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPasshash;

            _uow.SaveChanges();
        }

        public async Task<User> GetOAuthUserAsync(OAuthAccount oAuthAccount, string email)
        {
            var existingOAuthAccount = _uow.OAuthAccountRepository.GetOAuthAccountByProviderId(oAuthAccount.ProviderUserId,
                                                                        oAuthAccount.Provider);
            if (existingOAuthAccount != null)
                return existingOAuthAccount.User; // user and OAuth account exist

            var user = await _uow.UserRepository.GetByEmailAsync(email);
            if (user != null)
            {
                oAuthAccount.UserId = user.Id;
                _uow.OAuthAccountRepository.InsertGraph(oAuthAccount);

                _uow.SaveChanges();

                return user;
            }

            return null;
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}