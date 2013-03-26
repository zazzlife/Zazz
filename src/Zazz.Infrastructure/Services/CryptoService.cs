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
        private static readonly byte[] PasswordHashSecret = { 4, 166, 67, 133, 113, 2, 184, 158 }; //64 bit
        //Base64: BKZDhXECuJ4=

        /// <summary>
        /// This key should be used to sign a string to make sure it's not tampered with later.
        /// </summary>
        private static readonly byte[] RandomSignHashSecret =
            {
                109, 17, 99, 216, 57, 55, 193, 244, 152, 214, 126, 37,
                177, 169, 58, 211
            }; //128 bit
        //Base64: bRFj2Dk3wfSY1n4lsak60w==

        public string GeneratePasswordHash(string password)
        {
            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            return ComputeSHA1SignedHash(PasswordHashSecret, password);
        }

        public string GenerateTextSignature(string clearText)
        {
            if (String.IsNullOrEmpty(clearText))
                throw new ArgumentNullException("clearText");

            return ComputeSHA1SignedHash(RandomSignHashSecret, clearText);
        }

        public string ComputeSHA1SignedHash(byte[] secretKey, string clearText)
        {
            var hmacsha1 = new HMACSHA1(secretKey);
            var textBytes = Encoding.UTF8.GetBytes(clearText);

            var hash = hmacsha1.ComputeHash(textBytes);

            return Convert.ToBase64String(hash);
        }


        /*KEYS FOR LATER USE
         
         * 128 bit
         * Wmk9XcunbkZerlTkIPX8ng==
         * {90, 105, 61, 93, 203, 167, 110, 70, 94, 174, 84, 228, 32, 245, 252, 158}
         
         * 64 bit
         * eQCCZ7vEAwE=
         * {121, 0, 130, 103, 187, 196, 3, 1}
         
         */
    }
}