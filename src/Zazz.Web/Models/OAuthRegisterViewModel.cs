namespace Zazz.Web.Models
{
    public class OAuthRegisterViewModel : RegisterViewModel
    {
        public string OAuthProvidedData { get; set; }

        public string ProvidedDataSignature { get; set; }
    }
}