using System;
using System.Net;
using System.Web.Http;

namespace Zazz.Web.Controllers.Api
{
    public abstract class BaseApiController : ApiController
    {
        private int _currentUserId;
        protected int CurrentUserId
        {
            get
            {
                if (_currentUserId == 0)
                {
                    int userId;
                    if (Int32.TryParse(User.Identity.Name, out userId))
                    {
                        _currentUserId = userId;
                    }
                    else
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                }

                return _currentUserId;
            }
        }

         protected int ExtractUserIdFromHeader()
         {
             return Int32.Parse(Request.Headers.Authorization.Parameter);

             //var auth = Request.Headers.Authorization;
             //if (auth == null || String.IsNullOrEmpty(auth.Parameter))
             //    return 0;

             //var authSegments = auth.Parameter.Split(':');
             //if (authSegments.Length < 4)
             //    return 0;

             //int userId;
             //Int32.TryParse(authSegments[2], out userId);


             //return userId;
         }
    }
}