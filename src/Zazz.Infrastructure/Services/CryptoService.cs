using System;
using System.Security.Cryptography;
using System.Text;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CryptoService : ICryptoService
    {
        /// <summary>
        /// This key should be used to sign passwords in DB, using SHA1 (HMACSHA1)
        /// </summary>
        private static readonly byte[] PasswordHashSecret = { 4, 166, 67, 133, 113, 2, 184, 158 }; 
        //Base64: BKZDhXECuJ4=

        public string GeneratePasswordHash(string password)
        {
            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            var hmacsha1 = new HMACSHA1(PasswordHashSecret);
            var passBytes = Encoding.UTF8.GetBytes(password);

            var hash = hmacsha1.ComputeHash(passBytes);

            return Convert.ToBase64String(hash);
        }
    }
}