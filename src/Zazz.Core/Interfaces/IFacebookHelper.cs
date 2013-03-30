﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookHelper
    {
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
        IEnumerable<FbEvent> GetPageEvents(string pageId, string accessToken, int limit = 10);
        
        IEnumerable<FbPage> GetPages(string accessToken);

        void LinkPage(string pageId, string accessToken);

        string GetAlbumName(string albumId, string accessToken);

        IEnumerable<FbStatus> GetStatuses(string accessToken, int limit = 5);

        IEnumerable<FbPhoto> GetPhotos(string accessToken, int limit = 25);

        ZazzEvent FbEventToZazzEvent(FbEvent fbEvent);
    }
}