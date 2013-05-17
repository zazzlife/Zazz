namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        string EncryptText(string text, byte[] key);

        string DecryptText(string cipherText, byte[] key);

        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string GenerateSignedSHA1Hash(string clearText, string key);
    }
}