using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    /// <summary>
    /// This view model is used in _LayOut view
    /// </summary>
    public abstract class BaseLayoutViewModel
    {
        public string CurrentUserDisplayName { get; set; }

        public PhotoLinks CurrentUserPhoto { get; set; }
    }
}