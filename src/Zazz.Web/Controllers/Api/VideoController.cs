using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;
using System.Runtime.InteropServices;

namespace Zazz.Web.Controllers.Api
{
    public class VideoController : BaseApiController
    {


        // GET api/video/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/video
        public async Task<HttpResponseMessage> Post()
        {
            /*
            if (!Request.Content.IsMimeMultipartContent("form-data"))
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var streamProvider = new MultipartMemoryStreamProvider();            
            var bodyParts = await Request.Content.ReadAsMultipartAsync(streamProvider);

            // parsing video 
            var providedVideo = bodyParts.Contents
                .FirstOrDefault(c => c.Headers
                .ContentDisposition.Name.Equals("video", StringComparison.InvariantCultureIgnoreCase));

            if (providedVideo == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var videoStream = await providedVideo.ReadAsStreamAsync();



            String filename = "~/videos/"+CurrentUserId+"/"+DateTime.UtcNow.Millisecond;

            CopyStream(videoStream,filename);

            bool exists = System.IO.Directory.Exists("~/videos/" + CurrentUserId + "/");

            if (!exists)
                System.IO.Directory.CreateDirectory("~/videos/" + CurrentUserId + "/");
            */

            //int userid = CurrentUserId;
            String filename = "~/videos/1/1";
            String mime = getMimeFromFile(filename);

            String description = "";
            /*
            // parsing description
            var providedDescription = bodyParts.Contents
               .FirstOrDefault(c => c.Headers
               .ContentDisposition.Name.Equals("description", StringComparison.InvariantCultureIgnoreCase));

            if (providedDescription != null)
                description = await providedDescription.ReadAsStringAsync();

            */
            var response = Request.CreateResponse(HttpStatusCode.Created, mime);
            return response;
        }

        // PUT api/video/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/video/5
        public void Delete(int id)
        {
        }

        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }


        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        private string getMimeFromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found");

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                if (fs.Length >= 256)
                    fs.Read(buffer, 0, 256);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }
            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                return "unknown/unknown";
            }
        }
    }
}
