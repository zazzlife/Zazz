using FluentScheduler;
using StructureMap;

namespace Zazz.Web.BackgroundWorkers
{
    public class BackgroundTasksRegistry : Registry
    {
        public BackgroundTasksRegistry()
        {
            //TODO: Register tasks here
        }

        public override ITask GetTaskInstance<T>()
        {
            return ObjectFactory.Container.GetInstance<T>();
        }
    }
}