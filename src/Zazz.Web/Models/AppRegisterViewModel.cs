using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Zazz.Web.Models
{
    public class AppRegisterViewModel : RegisterViewModel
    {
        [HiddenInput]
        public long RequestId { get; set; }

        [HiddenInput]
        public string Token { get; set; }
    }
}