namespace Zazz.Web.Models
{
    public class FineUploadResponse
    {
        public bool Success { get; set; }

        public string Error { get; set; }

        public bool PreventRetry { get; set; }

        public int PhotoId { get; set; }

        public string PhotoUrl { get; set; }
    }
}