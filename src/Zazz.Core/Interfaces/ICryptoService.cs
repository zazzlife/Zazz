namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        string EncryptText(string text, byte[] key, out string iv);

        string DecryptText(string cipherText, string iv, byte[] key);

        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string GenerateSignedSHA1Hash(string clearText, string key);
    }
}