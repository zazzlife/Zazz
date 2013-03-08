using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookService
    {
        /// <summary>
        /// Gets the user info
        /// </summary>
        /// <param name="id">Id or username</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<FbUser> GetUserAsync(string id, string accessToken = null);

        /// <summary>
        /// Gets an event
        /// </summary>
        /// <param name="id">Event id</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<FbEvent> GetEventAsync(string id, string accessToken = null);

        /// <summary>
        /// Gets a post
        /// </summary>
        /// <param name="id">Post id</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<FbPost> GetPostAsync(string id, string accessToken = null);

        /// <summary>
        /// Gets the picture url
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="accessToken"></param>
        /// <returns>Url to image</returns>
        Task<string> GetPictureAsync(string objectId, int width, int height, string accessToken = null);

        /// <summary>
        /// Gets the picture url
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        /// <param name="pictureSize"></param>
        /// <param name="accessToken"></param>
        /// <returns>Url to image</returns>
        Task<string> GetPictureAsync(string objectId, PictureSize pictureSize, string accessToken = null);
    }
}