using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Zazz.Web.Controllers
{
    public class UploadController : Controller
    {
        public class UploadFileModel
        {
            public string name { get; set; }

            public string type { get; set; }

            public int size { get; set; }
        }

        public ActionResult Index()
        {
            throw new HttpException(404, "Not Found");
        }

        public JsonResult Image(HttpPostedFileBase image)
        {
            var result = new UploadFileModel
                             {
                                 name = image.FileName,
                                 size = image.ContentLength,
                                 type = image.ContentType
                             };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
