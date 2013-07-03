using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Zazz.Web
{
    public class OAuthErrorException : HttpResponseException
    {
        public OAuthErrorException(OAuthError error, string errorDescription = null,
                                   HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(statusCode)
        {}

        public OAuthErrorException(HttpResponseMessage response)
            : base(response)
        { }
    }

    public enum OAuthError
    {
        /// <summary>
        /// The request is missing a required parameter, includes an
        /// unsupported parameter value (other than grant type),
        /// repeats a parameter, includes multiple credentials,
        /// utilizes more than one mechanism for authenticating the
        /// client, or is otherwise malformed.
        /// </summary>
        invalid_request,
        /// <summary>
        /// Client authentication failed (e.g., unknown client, no
        /// client authentication included, or unsupported
        /// authentication method).  The authorization server MAY
        /// return an HTTP 401 (Unauthorized) status code to indicate
        /// which HTTP authentication schemes are supported.  If the
        /// client attempted to authenticate via the "Authorization"
        /// request header field, the authorization server MUST
        /// respond with an HTTP 401 (Unauthorized) status code and
        /// include the "WWW-Authenticate" response header field
        /// matching the authentication scheme used by the client.
        /// </summary>
        invalid_client,
        /// <summary>
        /// The provided authorization grant (e.g., authorization
        /// code, resource owner credentials) or refresh token is
        /// invalid, expired, revoked, does not match the redirection
        /// URI used in the authorization request, or was issued to
        /// another client.
        /// </summary>
        invalid_grant,
        /// <summary>
        /// The authenticated client is not authorized to use this
        /// authorization grant type.
        /// </summary>
        unauthorized_client,
        /// <summary>
        /// The authorization grant type is not supported by the
        /// authorization server.
        /// </summary>
        unsupported_grant_type,
        /// <summary>
        /// The requested scope is invalid, unknown, malformed, or
        /// exceeds the scope granted by the resource owner.
        /// </summary>
        invalid_scope

    }
}