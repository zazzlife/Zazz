namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        byte[] EncryptPassword(string password, out string iv);

        byte[] DecryptPassword(byte[] cipherBytes, byte[] iv);

        string EncryptText(string text, byte[] key, out string iv);

        string DecryptText(string cipherText, string iv, byte[] key);

        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string GenerateSignedSHA1Hash(string clearText, string key);

        byte[] GenerateKey(int keySizeInBits, bool generateNonZero = false);
    }
}