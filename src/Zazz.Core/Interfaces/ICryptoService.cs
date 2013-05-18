namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        byte[] EncryptPassword(string text);

        byte[] DecryptPassword(byte[] cipher);

        string EncryptText(string text, byte[] key, out string iv);

        string DecryptText(string cipherText, string iv, byte[] key);

        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string GenerateSignedSHA1Hash(string clearText, string key);

        byte[] GenerateKey(int keySizeInBits, bool generateNonZero = false);
    }
}