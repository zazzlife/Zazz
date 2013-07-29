using Zazz.Core.Models.Data;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Interfaces
{
    public interface IObjectMapper
    {
        ApiPost PostToApiPost(Post post);

        ApiPhoto PhotoToApiPhoto(Photo photo);

        ApiEvent EventToApiEvent(ZazzEvent e);

        ApiWeekly WeeklyToApiWeekly(Weekly weekly);

        ApiFeed FeedViewModelToApiModel(FeedViewModel feed);

        ApiComment CommentViewModelToApiModel(CommentViewModel commentViewModel);
    }
}