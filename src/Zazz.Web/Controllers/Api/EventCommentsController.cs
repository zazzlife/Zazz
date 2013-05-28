using Zazz.Core.Interfaces;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class EventCommentsController : BaseCommentsApiController
    {
        public EventCommentsController(ICommentService commentService)
            : base(commentService)
        { }
    }
}