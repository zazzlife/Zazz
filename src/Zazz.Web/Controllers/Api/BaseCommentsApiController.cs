using Zazz.Core.Interfaces;

namespace Zazz.Web.Controllers.Api
{
    public class BaseCommentsApiController : BaseApiController
    {
        protected readonly ICommentService CommentService;

        public BaseCommentsApiController(ICommentService commentService)
        {
            CommentService = commentService;
        }
    }
}