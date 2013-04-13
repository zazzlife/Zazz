namespace Zazz.Core.Models
{
    public class PhotoLinks
    {

        /// <summary>
        /// This one is for user image on comments and feeds and like that. It's around 55px
        /// </summary>
        public string VerySmallLink { get; set; }

        /// <summary>
        /// This one is for images on feeds and photo lists and like that. It's around 175px
        /// </summary>
        public string SmallLink { get; set; }

        /// <summary>
        /// This one is for images on feeds and like that. It's around 500px
        /// </summary>
        public string MediumLink { get; set; }

        /// <summary>
        /// This one is for when you show the image in full size, maximum size is around 1600px
        /// </summary>
        public string NormalLink { get; set; }
    }
}