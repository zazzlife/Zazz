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
    public class EventCommentsController : BaseApiController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly ICommentService _commentService;
        private readonly IObjectMapper _objectMapper;

        public EventCommentsController(IFeedHelper feedHelper, ICommentService commentService,
            IObjectMapper objectMapper)
        {
            _feedHelper = feedHelper;
            _commentService = commentService;
            _objectMapper = objectMapper;
        }

        // GET /api/v1/postcomments/5

        public IEnumerable<ApiComment> Get(int id, int lastComment = 0)
        {
            if (id == 0 || lastComment < 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return _feedHelper.GetComments(id, CommentType.Event, ExtractUserIdFromHeader(), lastComment, 10)
                              .Select(_objectMapper.CommentViewModelToApiModel);
        }

        // POST /api/v1/postcomments/5

        public ApiComment Post(int id, [FromBody] string comment)
        {
            if (id == 0 || String.IsNullOrWhiteSpace(comment))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var c = new Comment
                    {
                        Message = comment,
                        EventComment = new EventComment()
                                      {
                                          EventId = id
                                      },
                        Time = DateTime.UtcNow,
                        UserId = ExtractUserIdFromHeader()
                    };

            try
            {
                var cid = _commentService.CreateComment(c, CommentType.Event);

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