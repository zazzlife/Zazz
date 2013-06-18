using FluentScheduler;
using StructureMap;
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
        }
    }
}