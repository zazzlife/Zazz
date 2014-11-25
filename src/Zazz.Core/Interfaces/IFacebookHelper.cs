using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookHelper
    {
        FbBasicUserInfo GetBasicUserInfo(string accessToken);

        /// <summary>
        /// Use this only for getting user events until the FQL bug is fixed
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        IEnumerable<FbEvent> GetEvents(long creatorId, string accessToken, int limit = 5);

        /// <summary>
        /// Use this only for getting page events until the FQL bug is fixed
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        IEnumerable<FbEvent> GetPageEvents(string pageId, string accessToken);

        IEnumerable<FbEvent> GetUserAttendingEvents(string accessToken);
        
        IEnumerable<FbPage> GetPages(string accessToken);

        void LinkPage(string pageId, string accessToken);

        string GetAlbumName(string albumId, string accessToken);

        IEnumerable<FbStatus> GetStatuses(string accessToken);

        IEnumerable<FbPhoto> GetPhotos(string accessToken);

        IEnumerable<FbFriend> GetFriends(string accessToken);
        
        Task SendAppInviteRequests(IEnumerable<long> users, string appId, string appSecret, string message);

        ZazzEvent FbEventToZazzEvent(FbEvent fbEvent);

        FbPage GetpageDetails(string pageId, string accessToken);
    }
}