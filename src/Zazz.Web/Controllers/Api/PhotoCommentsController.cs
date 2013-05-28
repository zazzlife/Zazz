using Zazz.Core.Interfaces;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PhotoCommentsController : BaseCommentsApiController
    {
        public PhotoCommentsController(ICommentService commentService)
            : base(commentService)
        { }
    }
}