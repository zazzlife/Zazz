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
    }
}