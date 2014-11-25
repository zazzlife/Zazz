namespace Zazz.Core.Models.Facebook
{
    public class FbPage
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string AcessToken { get; set; }

        public string email { get; set; }

        public FbCover fbCover { get; set; }

        public string profilePic { get; set; }

        public string url { get; set; }

        public string username { get; set; }

        public FbLocation location { get; set; }
    }
}