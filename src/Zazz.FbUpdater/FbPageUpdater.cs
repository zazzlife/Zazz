using System.Threading.Tasks;

namespace Zazz.FbUpdater
{
    public class FbPageUpdater
    {
        public Task StartUpdate()
        {
            var tsc = new TaskCompletionSource<object>();
            tsc.SetResult(null);
            return tsc.Task;
        }
    }
}