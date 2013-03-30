using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Zazz.Core.Models.Facebook
{
    [DataContract]
    public class FbPhoto
    {
        public string Id { get; set; }

        public string AlbumId { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public long CreatedTime { get; set; }
    }
}