using System;
using System.Security.Cryptography;
using System.Text;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    public class CryptoService : ICryptoService
    {
        /// <summary>
        /// This key should be used only for hashing passwords
        /// </summary>
        private const string PASSWORD_HASH_SECRET = "MbxTE7mlyrorVRZOI3G2wYX7fZqjCJnM7mDhfI6iqmQ9QOzAsvqe58uhYDlx20rG2XFfdcQ/Aa+yzPRZu7FR6A==";
        private readonly byte[] _passwordHashSecret;

        /// <summary>
        /// This key should be used to encrypt passwords in DB, RijndaelManaged (AES)
        /// </summary>
        private const string PASSWORD_CIPHER_KEY = "aRRuXfnGkKR8NTnco+Bu9ts3kLGUS4Jp3RUSsCe/pWk=";
        private readonly byte[] _passwordCipherKeyBuffer;
        
        /// <summary>
        /// This key should be used to sign a string to make sure it's not tampered with later.
        /// </summary>
        private readonly byte[] _randomSignHashSecretBuffer;
        private const string RANDOM_SIGN_HASH_SECRET = "NtuWQJzOgoAtRQwZkXtUedaD/rDILB2vjGYQzfWmx91xLolrVP4Gtm9aBDGaW+NHXxGFu/7pRKg8a1H0aU1bpBISrXvAQ+gS8+Qk7BxqJ+gP3qDV4wwZdrHG8d3SoJnZJ3smlXsKT+oIpPrKFfVmK71yElTJAmVvk9uSaKW6l40=";

        // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
        public CryptoService()
        {
            _passwordCipherKeyBuffer = Convert.FromBase64String(PASSWORD_CIPHER_KEY);
            _randomSignHashSecretBuffer = Convert.FromBase64String(RANDOM_SIGN_HASH_SECRET);
            _passwordHashSecret = Convert.FromBase64String(PASSWORD_HASH_SECRET);
        }

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

        public byte[] EncryptPassword(string password, out string iv)
        {
            using (var cipher = CreateCipher(_passwordCipherKeyBuffer))
            using (var cryptoTransform = cipher.CreateEncryptor())
            {
                iv = Convert.ToBase64String(cipher.IV);
                var textBytes = Encoding.UTF8.GetBytes(password);
                return cryptoTransform.TransformFinalBlock(textBytes, 0, password.Length);
            }
        }

        public string DecryptPassword(byte[] cipherBytes, byte[] iv)
        {
            using (var cipher = CreateCipher(_passwordCipherKeyBuffer))
            {
                cipher.IV = iv;
                using (var cryptoTransform = cipher.CreateDecryptor())
                {
                    return Encoding.UTF8.GetString(cryptoTransform
                        .TransformFinalBlock(cipherBytes, 0, cipherBytes.Length));
                }
            }
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

        public string GenerateTextSignature(string clearText)
        {
            if (String.IsNullOrEmpty(clearText))
                throw new ArgumentNullException("clearText");

            var textBuffer = Encoding.UTF8.GetBytes(clearText);
            return GenerateHMACSHA512Hash(textBuffer, _randomSignHashSecretBuffer);
        }

        public string GenerateHexTextSignature(string clearText)
        {
            if (String.IsNullOrEmpty(clearText))
                throw new ArgumentNullException("clearText");

            var textBuffer = Encoding.UTF8.GetBytes(clearText);
            using (var hmacsha512 = new HMACSHA512(_randomSignHashSecretBuffer))
            {
                var hash = hmacsha512.ComputeHash(textBuffer);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        
        public string GenerateHMACSHA1Hash(string clearText, string key)
        {
            using (var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(clearText));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public string GenerateHMACSHA1Hash(byte[] text, byte[] key)
        {
            using (var hmacsha1 = new HMACSHA1(key))
            {
                var hash = hmacsha1.ComputeHash(text);
                return Convert.ToBase64String(hash);
            }
        }

        public string GenerateHMACSHA512Hash(byte[] text, byte[] key)
        {
            using (var hmacsha512 = new HMACSHA512(key))
            {
                var hash = hmacsha512.ComputeHash(text);
                return Convert.ToBase64String(hash);
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
    }
}