using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zazz.Infrastructure.Helpers
{
    public class RegexHelper
    {
        public IEnumerable<string> ExtractTags(string text)
        {
            return from Match match in Regex.Matches(text, @"(?<!\w)#\w+(-?\w+)")
                   select match.Value;
        }
    }
}