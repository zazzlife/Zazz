using Zazz.Core.Attributes;

namespace Zazz.Core.Models
{
    public class PhotoLinks
    {
        /// <summary>
        /// This one is for user image on comments and feeds and like that. It's around 55px
        /// </summary>
        [Photo(Height = 55, Width = 55, Quality = 95, Suffix = "vs")]
        public string VerySmallLink { get; set; }

        /// <summary>
        /// This one is for images on feeds and photo lists and like that. It's around 175px
        /// </summary>
        [Photo(Height = 175, Width = 175, Quality = 85, Suffix = "s")]
        public string SmallLink { get; set; }

        /// <summary>
        /// This one is for images on feeds and like that. It's around 500px
        /// </summary>
        [Photo(Height = 500, Width = 500, Quality = 70, Suffix = "m")]
        public string MediumLink { get; set; }

        /// <summary>
        /// This one is for when you show the image in full size, maximum size is around 1600px
        /// </summary>
        [Photo(Height = 1600, Width = 1600, Quality = 60)]
        public string OriginalLink { get; set; }

        public PhotoLinks()
        { }

        public PhotoLinks(string defaultValue)
        {
            VerySmallLink = defaultValue;
            SmallLink = defaultValue;
            MediumLink = defaultValue;
            OriginalLink = defaultValue;
        }
    }
}