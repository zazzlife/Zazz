using System;

namespace Zazz.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PhotoAttribute : Attribute
    {
        public string Suffix { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public long Quality { get; set; }

    }
}