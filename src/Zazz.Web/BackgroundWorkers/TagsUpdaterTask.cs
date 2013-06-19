using FluentScheduler;
using StructureMap.Pipeline;
using Zazz.Core.Interfaces;

namespace Zazz.Web.BackgroundWorkers
{
    public class TagsUpdaterTask : ITask
    {
        private readonly ITagService _tagService;

        public TagsUpdaterTask(ITagService tagService)
        {
            _tagService = tagService;
        }

        public void Execute()
        {
            _tagService.UpdateTagStatistics();
            
            new HybridLifecycle().FindCache().DisposeAndClear();
        }
    }
}