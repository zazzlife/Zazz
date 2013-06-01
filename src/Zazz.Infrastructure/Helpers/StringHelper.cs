using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Helpers
{
    // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
    public class StringHelper : IStringHelper
    {
        public IEnumerable<string> ExtractTags(string text)
        {
            return from Match match in Regex.Matches(text, @"(?<!\w)#\w+(-?\w+)")
                   select match.Value;
        }
    }
}