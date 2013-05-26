using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class TagsPageViewModel
    {
        public IEnumerable<string> AvailableTags { get; set; }

        public IEnumerable<string> SelectedTags { get; set; }

        public IEnumerable<FeedViewModel> Feeds { get; set; }
    }
}