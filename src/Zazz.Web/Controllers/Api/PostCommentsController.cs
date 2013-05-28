using Zazz.Core.Interfaces;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PostCommentsController : BaseApiController
    {}
}