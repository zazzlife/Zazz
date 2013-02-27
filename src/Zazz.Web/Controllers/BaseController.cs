using System;
using System.Web.Mvc;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class BaseController : Controller
    {
        public void ShowAlert(string message, AlertType alertType)
        {
            TempData["alert"] = true;
            TempData["alertMessage"] = message;

            var alertClass = String.Empty;

            if (alertType == AlertType.Error)
                alertClass = "alert-error";
            else if (alertType == AlertType.Success)
                alertClass = "alert-success";
            else if (alertType == AlertType.Warning)
                alertClass = "alert-warning";
            else if (alertType == AlertType.Info)
                alertClass = "alert-info";

            TempData["alertClass"] = alertClass;
        }
    }
}