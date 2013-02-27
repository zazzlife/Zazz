using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CryptoService : ICryptoService
    {
        
        /// <summary>
        /// This key should be used to sign passwords in DB, using SHA1 (HMACSHA1)
        /// </summary>
        private static byte[] _passwordHashSecret = { 4, 166, 67, 133, 113, 2, 184, 158 };
        //Base64: BKZDhXECuJ4=

        public string GeneratePasswordHash(string password)
        {
            throw new System.NotImplementedException();
        }
    }
}