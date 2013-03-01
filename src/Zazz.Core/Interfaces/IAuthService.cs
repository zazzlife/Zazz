using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAuthService : IDisposable
    {
        Task LoginAsync(string username, string password);

        Task<User> RegisterAsync(User user, bool createToken);

        Task<ValidationToken> GenerateResetPasswordTokenAsync(string email);

        Task<bool> IsTokenValidAsync(int userId, Guid token);

        Task ResetPasswordAsync(int userId, Guid token, string newPassword);
        
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        
        Task<User> GetOAuthUser(long oauthId, string email);
    }
}