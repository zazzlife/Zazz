namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        byte[] EncryptPassword(string password, out string iv);

        string DecryptPassword(byte[] cipherBytes, byte[] iv);

        string EncryptText(string text, byte[] key, out string iv);

        string DecryptText(string cipherText, string iv, byte[] key);

        string GenerateTextSignature(string clearText);

        string GenerateHexTextSignature(string clearText);

        string GenerateQRCodeToken(byte[] userPassword);

        string GenerateHMACSHA1Hash(string clearText, string key);

        string GenerateHMACSHA512Hash(byte[] text, byte[] key);

        byte[] GenerateKey(int keySizeInBits, bool generateNonZero = false);
    }
}