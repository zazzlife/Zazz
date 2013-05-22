using System;
using System.Web.Http;

namespace Zazz.Web.Controllers.Api
{
    public abstract class BaseApiController : ApiController
    {
         int ExtractUserIdFromHeader()
         {
             var auth = Request.Headers.Authorization;
             if (auth == null || String.IsNullOrEmpty(auth.Parameter))
                 return 0;

             var authSegments = auth.Parameter.Split(':');
             if (authSegments.Length < 4)
                 return 0;

             int userId;
             Int32.TryParse(authSegments[2], out userId);

             return userId;
         }
    }
}