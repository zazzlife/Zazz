using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbImage
    {
        /// <summary>
        /// Url to the original image.
        /// </summary>
        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }
    }
}