using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class CategoriesPageViewModel
    {
        public IEnumerable<string> AvailableCategories { get; set; }

        public IEnumerable<string> SelectedCategories { get; set; }

        public IEnumerable<FeedViewModel> Feeds { get; set; }
    }
}