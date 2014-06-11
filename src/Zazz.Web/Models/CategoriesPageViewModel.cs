using System.Collections.Generic;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class CategoriesPageViewModel
    {
        public bool HasFacebookAccount { get; set; }

        public AccountType AccountType { get; set; }

        public IEnumerable<string> AvailableCategories { get; set; }

        public IEnumerable<string> SelectedCategories { get; set; }

        public FeedsViewModel Feeds { get; set; }
    }
}