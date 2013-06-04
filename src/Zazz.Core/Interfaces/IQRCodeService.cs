using System.IO;

namespace Zazz.Core.Interfaces
{
    public interface IQRCodeService
    {
        MemoryStream Generate(string text);
    }
}