using System;

namespace Zazz.Web.Helpers
{
    public static class UriHelper
    {
        public static string GetBaseAddress(this Uri uri)
        {
            return uri.Scheme + "://" + uri.Authority;
        }
    }
}