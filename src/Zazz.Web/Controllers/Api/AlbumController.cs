using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;

namespace Zazz.Web.Controllers.Api
{
    public class AlbumController : BaseApiController
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        // GET api/v1/album/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/album
        public void Post([FromBody]string value)
        {
        }

        // PUT api/album/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/album/5
        public void Delete(int id)
        {
        }
    }
}
