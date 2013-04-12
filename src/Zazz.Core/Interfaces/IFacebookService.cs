using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookService : IDisposable
    {
        void HandleRealtimeUserUpdatesAsync(FbUserChanges changes);

        //Task HandleRealtimePageUpdatesAsync(FbPageChanges changes);

        void UpdateUserEvents(long fbUserId, int limit = 5);

        void UpdatePageEvents(string pageId, int limit = 10);

        void UpdatePageStatuses(string pageId, int limit = 25);

        void UpdatePagePhotos(string pageId, int limit = 25);

        IEnumerable<FbPage> GetUserPages(int userId);

        void LinkPage(FacebookPage fbPage);

        Task UnlinkPage(string fbPageId, int currentUserId);
    }
}