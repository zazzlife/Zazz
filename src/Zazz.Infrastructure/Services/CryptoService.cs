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

        private RijndaelManaged CreateCipher(byte[] key)
        {
            var cipher = new RijndaelManaged
                         {
                             BlockSize = 128,
                             Key = key,
                             Mode = CipherMode.CBC,
                             Padding = PaddingMode.ISO10126
                         };

            return cipher;
        }

        public string EncryptText(string text, byte[] key, out string iv)
        {
            using (var cipher = CreateCipher(key))
            using (var cryptoTransform = cipher.CreateEncryptor())
            {
                iv = Convert.ToBase64String(cipher.IV);
                var textBytes = Encoding.UTF8.GetBytes(text);
                var cipherText = cryptoTransform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                return Convert.ToBase64String(cipherText);
            }
        }

        public string DecryptText(string cipherText, string iv, byte[] key)
        {
            using (var cipher = CreateCipher(key))
            {
                cipher.IV = Convert.FromBase64String(iv);
                using (var cryptoTransform = cipher.CreateDecryptor())
                {
                    var cipherBytes = Convert.FromBase64String(cipherText);
                    var textBytes = cryptoTransform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                    return Encoding.UTF8.GetString(textBytes);
                }
            }
        }

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

        public string GenerateSignedSHA1Hash(string clearText, string key)
        {
            using (var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(clearText));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public byte[] GenerateKey(int keySizeInBits, bool generateNonZero = false)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var key = new byte[keySizeInBits/8];

                if (generateNonZero)
                {
                    rngCryptoServiceProvider.GetNonZeroBytes(key);
                }
                else
                {
                    rngCryptoServiceProvider.GetBytes(key);
                }

                return key;
            }
        }

        private string ComputeSHA1SignedHash(byte[] secretKey, string clearText)
        {
            using (var hmacsha1 = new HMACSHA1(secretKey))
            {
                var textBytes = Encoding.UTF8.GetBytes(clearText);
                var hash = hmacsha1.ComputeHash(textBytes);

                return Convert.ToBase64String(hash);
            }
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