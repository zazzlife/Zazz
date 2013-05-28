using System;
using System.Net;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class CommentsController : BaseCommentsApiController
    {
        public CommentsController(ICommentService commentService) : base(commentService)
        {}

        public void Put(int id, [FromBody] ApiComment comment)
        {
            try
            {
                CommentService.EditComment(id, ExtractUserIdFromHeader(), comment.CommentText);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}