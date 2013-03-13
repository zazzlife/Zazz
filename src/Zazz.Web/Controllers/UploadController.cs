using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class UploadController : BaseController
    {
        public ActionResult Index()
        {
            throw new HttpException(404, "Not Found");
        }

        public JsonResult ImageAjax(HttpPostedFileBase image)
        {
            var result = new UploadFileModel
                             {
                                 name = image.FileName,
                                 size = image.ContentLength,
                                 type = image.ContentType
                             };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class UploadFileModel
        {
            public string name { get; set; }

            public string type { get; set; }

            public int size { get; set; }
        }
    }
}
