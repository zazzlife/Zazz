using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("OAuthRefreshToken_Scopes")]
    public class OAuthRefreshTokenScope
    {
        [ForeignKey("RefreshToken"), Key, Column(Order = 0)]
        public int RefreshTokenId { get; set; }

        [ForeignKey("Scope"), Key, Column(Order = 1)]
        public byte ScopeId { get; set; }

        public OAuthRefreshToken RefreshToken { get; set; }

        public OAuthScope Scope { get; set; }
    }
}