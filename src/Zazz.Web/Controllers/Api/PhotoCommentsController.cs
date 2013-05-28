using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PhotoCommentsController : BaseApiController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly ICommentService _commentService;

        public PhotoCommentsController(IFeedHelper feedHelper, ICommentService commentService)
        {
            _feedHelper = feedHelper;
            _commentService = commentService;
        }

        // GET /api/v1/photocomments/5

        public IEnumerable<ApiComment> Get(int id, int lastComment = 0)
        {
            if (id == 0 || lastComment < 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return _feedHelper.GetComments(id, CommentType.Photo, ExtractUserIdFromHeader(), lastComment)
                              .Select(_feedHelper.CommentViewModelToApiModel);
        }

        // POST /api/v1/photocomments/5

        public ApiComment Post(int id, [FromBody] string comment)
        {
            if (id == 0 || String.IsNullOrWhiteSpace(comment))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var c = new Comment
                    {
                        Message = comment,
                        PhotoComment = new PhotoComment
                                       {
                                           PhotoId = id
                                       },
                        Time = DateTime.UtcNow,
                        UserId = ExtractUserIdFromHeader()
                    };

            try
            {
                var cid = _commentService.CreateComment(c, CommentType.Photo);

                return new ApiComment
                       {
                           CommentId = cid,
                           CommentText = comment,
                           IsFromCurrentUser = true,
                           Time = c.Time,
                           UserId = ExtractUserIdFromHeader()
                       };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}