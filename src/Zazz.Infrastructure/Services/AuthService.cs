using System;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUoW _uoW;
        private readonly ICryptoService _cryptoService;

        public AuthService(IUoW uoW, ICryptoService cryptoService)
        {
            _uoW = uoW;
            _cryptoService = cryptoService;
        }

        public async Task LoginAsync(string username, string password)
        {
            var passwordHash = _cryptoService.GeneratePasswordHash(password);
            var user = await _uoW.UserRepository.GetByUsernameAsync(username);

            if (user == null)
                throw new UserNotExistsException();

            if (passwordHash != user.Password)
                throw new InvalidPasswordException();

            user.LastActivity = DateTime.UtcNow;

            await _uoW.SaveAsync();
        }

        public async Task<User> RegisterAsync(User user, bool createToken)
        {
            if (user.UserDetail == null)
                throw new ArgumentNullException();

            var usernameExists = await _uoW.UserRepository.ExistsByUsernameAsync(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = await _uoW.UserRepository.ExistsByEmailAsync(user.Email);
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

            _uoW.UserRepository.InsertGraph(user);

            await _uoW.SaveAsync();

            return user;
        }

        public async Task<ValidationToken> GenerateResetPasswordTokenAsync(string email)
        {
            var userId = await _uoW.UserRepository.GetIdByEmailAsync(email);
            if (userId == 0)
                throw new EmailNotExistsException();

            var token = new ValidationToken
                            {
                                ExpirationDate = DateTime.UtcNow.AddDays(1),
                                Id = userId,
                                Token = Guid.NewGuid(),
                            };

            var oldToken = await _uoW.ValidationTokenRepository.GetByIdAsync(userId);
            if (oldToken != null)
                _uoW.ValidationTokenRepository.Remove(oldToken);

            _uoW.ValidationTokenRepository.InsertGraph(token);
            await _uoW.SaveAsync();

            return token;
        }

        public async Task<bool> IsTokenValidAsync(int userId, Guid token)
        {
            var userToken = await _uoW.ValidationTokenRepository.GetByIdAsync(userId);
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

            var user = await _uoW.UserRepository.GetByIdAsync(userId);
            var newPassHash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPassHash;
            await _uoW.ValidationTokenRepository.RemoveAsync(user.Id);
            await _uoW.SaveAsync();
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _uoW.UserRepository.GetByIdAsync(userId);

            var currentPassHash = _cryptoService.GeneratePasswordHash(currentPassword);
            if (currentPassHash != user.Password)
                throw new InvalidPasswordException();

            var newPasshash = _cryptoService.GeneratePasswordHash(newPassword);

            user.Password = newPasshash;

            await _uoW.SaveAsync();
        }

        public async Task<User> GetOAuthUserAsync(OAuthAccount oAuthAccount, string email)
        {
            var existingOAuthAccount = _uoW.OAuthAccountRepository.GetOAuthAccountByProviderId(oAuthAccount.ProviderUserId,
                                                                        oAuthAccount.Provider);
            if (existingOAuthAccount != null)
                return existingOAuthAccount.User; // user and OAuth account exist

            var user = await _uoW.UserRepository.GetByEmailAsync(email);
            if (user != null)
            {
                oAuthAccount.UserId = user.Id;
                _uoW.OAuthAccountRepository.InsertGraph(oAuthAccount);

                await _uoW.SaveAsync();

                return user;
            }

            return null;
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}