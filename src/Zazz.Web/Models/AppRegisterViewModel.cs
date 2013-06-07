using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Zazz.Web.Models
{
    public class AppRegisterViewModel : RegisterViewModel
    {
        [HiddenInput]
        public string App { get; set; }

        [HiddenInput]
        public string AppSignature { get; set; }
    }
}