using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookService : IDisposable
    {
        Task HandleRealtimeUserUpdatesAsync(FbUserChanges changes);

        //Task HandleRealtimePageUpdatesAsync(FbPageChanges changes);

        Task UpdateUserEventsAsync(long fbUserId);

        Task UpdatePageEventsAsync(string pageId);

        void UpdatePageStatusesAsync(string pageId);

        void UpdatePagePhotosAsync(string pageId);

        Task<IEnumerable<FbPage>> GetUserPagesAsync(int userId);

        void LinkPage(FacebookPage fbPage);

        void UnlinkPage(string fbPageId, int currentUserId);
    }
}