using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IStaticDataRepository
    {
        IEnumerable<School> GetSchools();
        IEnumerable<City> GetCities();
        IEnumerable<Major> GetMajors();
        IEnumerable<Tag> GetTags();

        Tag GetTagIfExists(string tagName);
    }
}