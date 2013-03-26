namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string GenerateSignedSHA1Hash(string clearText, string key);
    }
}