using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    public class KeyChain : IKeyChain
    {
        public string FACEBOOK_REALTIME_VERIFY_TOKEN { get; set; }
        public string FACEBOOK_APP_ID { get; set; }
        public string FACEBOOK_API_SECRET { get; set; }

        public KeyChain(string facebookRealtimeToken, string facebookAppId, string facebookAppSecret)
        {
            FACEBOOK_REALTIME_VERIFY_TOKEN = facebookRealtimeToken;
            FACEBOOK_APP_ID = facebookAppId;
            FACEBOOK_API_SECRET = facebookAppSecret;
        }
    }
}