namespace Zazz.Core.Interfaces
{
    public interface IKeyChain
    {
        string FACEBOOK_REALTIME_VERIFY_TOKEN { get; set; }

        string FACEBOOK_APP_ID { get; set; }

        string FACEBOOK_API_SECRET { get; set; }
    }
}