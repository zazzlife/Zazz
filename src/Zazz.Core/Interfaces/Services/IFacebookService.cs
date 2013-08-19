using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces.Services
{
    public interface IFacebookService
    {
        void HandleRealtimeUserUpdatesAsync(FbUserChanges changes);

        //Task HandleRealtimePageUpdatesAsync(FbPageChanges changes);

        void UpdateUserEvents(long fbUserId, int limit = 5);

        void SyncPageEvents(string pageId, int limit = 10);

        void SyncPageStatuses(string pageFbId, int limit = 25);

        void SyncPagePhotos(string pageId, int limit = 25);

        IEnumerable<FbPage> GetUserPages(int userId);

        void LinkPage(FacebookPage fbPage);

        void UnlinkPage(string fbPageId, int currentUserId);

        void UpdatePagesAccessToken(int userId, string accessToken);

        IQueryable<User> FindZazzFbFriends(string accessToken);
    }
}