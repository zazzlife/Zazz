using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IUoW _uow;

        public PostController(IUoW uow)
        {
            _uow = uow;
        }

        [Authorize, HttpPost]
        public async Task<ActionResult> New(string message)
        {
            using (_uow)
            {
                if (String.IsNullOrEmpty(message))
                    throw new ArgumentNullException("message");

                var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);
                var post = new Post
                           {
                               CreatedTime = DateTime.UtcNow,
                               Message = message,
                               UserId = userId
                           };

                return View("FeedItems/_PostFeedItem");
            }
        }
    }
}
