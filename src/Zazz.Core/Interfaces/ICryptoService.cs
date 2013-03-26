namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);

        string ComputeSHA1SignedHash(byte[] secretKey, string clearText);
    }
}