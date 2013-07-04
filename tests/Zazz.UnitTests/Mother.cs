using System;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests
{
    public static class Mother
    {
         public static OAuthClient GetApiApp()
         {
             return new OAuthClient
             {
                 Id = 1,
                 IsAllowedToRequestFullScope = true,
                 ClientId = "client id",
                 IsAllowedToRequestPasswordGrantType = true,
                 Name = "name",
                 Secret = "secret"
             };
         }
    }
}