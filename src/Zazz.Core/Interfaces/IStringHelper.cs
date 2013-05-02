using System.Collections.Generic;

namespace Zazz.Core.Interfaces
{
    public interface IStringHelper
    {
        IEnumerable<string> ExtractTags(string text);
    }
}