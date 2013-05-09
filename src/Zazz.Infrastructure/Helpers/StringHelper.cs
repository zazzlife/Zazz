using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Helpers
{
    public class StringHelper : IStringHelper
    {
        public IEnumerable<string> ExtractTags(string text)
        {
            return from Match match in Regex.Matches(text, @"(?<!\w)#\w+(-?\w+)")
                   select match.Value;
        }

        public string WrapTagsInAnchorTag(string text)
        {
            var tags = ExtractTags(text);

            foreach (var tag in tags)
            {
                var anchorTag = String.Format("<a class='tag' href='/home/tags?select={0}'>{1}</a>",
                                              tag.Replace("#", ""), tag);

                text = text.Replace(tag, anchorTag);
            }

            return text;
        }
    }
}