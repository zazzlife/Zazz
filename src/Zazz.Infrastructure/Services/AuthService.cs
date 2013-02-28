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
            var usernameExists = await _uoW.UserRepository.ExistsByUsernameAsync(user.Username);
            if (usernameExists)
                throw new UsernameExistsException();

            var emailExists = await _uoW.UserRepository.ExistsByEmailAsync(user.Email);
            if (emailExists)
                throw new EmailExistsException();

            var hashedPassword = _cryptoService.GeneratePasswordHash(user.Password);
            user.Password = hashedPassword;

            user.JoinedDate = DateTime.UtcNow;

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

        public Task GenerateResetPasswordTokenAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsTokenValidAsync(int userId, Guid token)
        {
            throw new NotImplementedException();
        }

        public Task ResetPasswordAsync(int userId, Guid token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}