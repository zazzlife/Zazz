using System;
using System.Net;
using System.Security;
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
            if (comment == null || String.IsNullOrWhiteSpace(comment.CommentText))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                CommentService.EditComment(id, ExtractUserIdFromHeader(), comment.CommentText);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        public void Delete(int id)
        {
            
        }
    }
}