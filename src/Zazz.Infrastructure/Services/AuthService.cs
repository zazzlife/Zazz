using System;
using System.Threading.Tasks;
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

        public Task LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(User user)
        {
            throw new NotImplementedException();
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
    }
}