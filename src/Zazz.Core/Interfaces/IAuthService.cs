using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IAuthService
    {
        void Login(string username, string password);

        User Register(User user,string password, bool createToken);

        UserValidationToken GenerateResetPasswordToken(string email);

        bool IsTokenValid(int userId, string token);

        void ResetPassword(int userId, string token, string newPassword);
        
        void ChangePassword(int userId, string currentPassword, string newPassword);
        
        User GetOAuthUser(LinkedAccount linkedAccount, string email);
        
        void AddOrUpdateOAuthAccount(LinkedAccount oauthAccount);
    }
}