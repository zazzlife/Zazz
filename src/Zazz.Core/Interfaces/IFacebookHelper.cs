using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookHelper
    {
        Task<T> GetAsync<T>(string path) where T : class;

        Task<T> GetAsync<T>(string path, params string[] fieldsToGet) where T : class;
    }
}