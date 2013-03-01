using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class OAuthLoginResponse
    {
        public long ProviderUserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string AccessToken { get; set; }

        public OAuthProvider Provider { get; set; }
    }
}