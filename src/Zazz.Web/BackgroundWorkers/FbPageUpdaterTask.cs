using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FluentScheduler;
using StructureMap.Pipeline;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Web.BackgroundWorkers
{
    public class FbPageUpdaterTask : ITask
    {
        private readonly IFacebookService _facebookService;
        private readonly IUoW _uow;

        public FbPageUpdaterTask(IFacebookService facebookService, IUoW uow)
        {
            _facebookService = facebookService;
            _uow = uow;
        }

        public void Execute()
        {
            // NOTE: don't use this method if the pages grow over 1000
            var pages = _uow.FacebookPageRepository
                .GetAll()
                .Select(p => p.FacebookId)
                .ToList();

            foreach (var page in pages)
            {
                // not sending them in parallel so facebook won't block us!
                _facebookService.SyncPageEvents(page);
                _facebookService.UpdatePagePhotos(page);
                _facebookService.SyncPageStatuses(page);

                Task.Delay(1000).Wait();
            }

            new HybridLifecycle().FindCache().DisposeAndClear();
        }
    }
}