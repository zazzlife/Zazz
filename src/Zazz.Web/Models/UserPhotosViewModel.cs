using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserPhotosViewModel : UserProfileViewModelBase
    {
        public IEnumerable<PhotoViewModel> Photos { get; set; }
    }
}