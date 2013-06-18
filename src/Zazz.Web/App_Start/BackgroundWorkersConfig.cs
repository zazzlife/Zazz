using FluentScheduler;
using Zazz.Web.BackgroundWorkers;

namespace Zazz.Web
{
    public static class BackgroundWorkersConfig
    {
         public static void SetupWorkers()
         {
             TaskManager.Initialize(new BackgroundTasksRegistry());
         }
    }
}