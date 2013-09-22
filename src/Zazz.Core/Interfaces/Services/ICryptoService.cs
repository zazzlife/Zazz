namespace Zazz.Core.Interfaces.Services
{
    public interface ICryptoService
    {
        /// <summary>
        /// Returns hash of the password in base64 encoded format
        /// </summary>
        /// <param name="password">password in clear text</param>
        /// <returns></returns>
        string GeneratePasswordHash(string password);

        byte[] EncryptPassword(string password, out string iv);

        string DecryptPassword(byte[] cipherBytes, byte[] iv);

        string EncryptText(string text, byte[] key, out string iv);

        string DecryptText(string cipherText, string iv, byte[] key);

        string GenerateTextSignature(string clearText);

        string GenerateHexTextSignature(string clearText);

        /// <summary>
        /// Returns HEX
        /// </summary>
        /// <param name="clearText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GenerateHMACSHA1Hash(string clearText, string key);

        /// <summary>
        /// Returns Base64
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GenerateHMACSHA1Hash(byte[] text, byte[] key);

        /// <summary>
        /// Returns Base64
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GenerateHMACSHA512Hash(byte[] text, byte[] key);

        byte[] GenerateKey(int keySizeInBits, bool generateNonZero = false);
    }
}