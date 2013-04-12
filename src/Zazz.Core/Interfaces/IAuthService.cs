using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAuthService : IDisposable
    {
        void Login(string username, string password);

        User Register(User user, bool createToken);

        ValidationToken GenerateResetPasswordToken(string email);

        bool IsTokenValid(int userId, Guid token);

        void ResetPassword(int userId, Guid token, string newPassword);
        
        void ChangePassword(int userId, string currentPassword, string newPassword);
        
        User GetOAuthUser(OAuthAccount oAuthAccount, string email);
        
        void AddOAuthAccount(OAuthAccount oauthAccount);
    }
}