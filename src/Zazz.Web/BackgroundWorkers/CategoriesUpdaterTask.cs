using FluentScheduler;
using StructureMap.Pipeline;
using Zazz.Core.Interfaces;

namespace Zazz.Web.BackgroundWorkers
{
    public class CategoriesUpdaterTask : ITask
    {
        private readonly ICategoryService _categoryService;

        public CategoriesUpdaterTask(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public void Execute()
        {
            _categoryService.UpdateStatistics();
            
            new HybridLifecycle().FindCache().DisposeAndClear();
        }
    }
}