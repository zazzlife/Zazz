namespace Zazz.Core.Interfaces
{
    public interface ICryptoService
    {
        string GeneratePasswordHash(string password);

        string GenerateTextSignature(string clearText);
    }
}