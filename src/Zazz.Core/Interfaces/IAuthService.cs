using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAuthService
    {
        Task LoginAsync(string username, string password);

        Task RegisterAsync(User user);

        Task GenerateResetPasswordTokenAsync(string email);

        Task<bool> IsTokenValidAsync(int userId, int token, string tokenHash);

        Task ResetPasswordAsync(int userId, int token, string tokenHash, string newPassword);
        
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}