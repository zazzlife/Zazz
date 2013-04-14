using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    /// <summary>
    /// This view model is used in _LayOut view
    /// </summary>
    public abstract class BaseLayoutViewModel
    {
        public string UserDisplayName { get; set; }

        public PhotoLinks UserPhoto { get; set; }
    }
}