using System;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.FbUpdater
{
    public class FbPageUpdater
    {
        private readonly IFacebookService _facebookService;
        private readonly IUoW _uow;

        public FbPageUpdater(IFacebookService facebookService, IUoW uow)
        {
            _facebookService = facebookService;
            _uow = uow;
        }

        public async Task StartUpdate()
        {
            // NOTE: don't use this method if the pages grow over 1000
            var pages = _uow.FacebookPageRepository
                .GetAll()
                .Select(p => p.FacebookId)
                .ToList();

            var counter = 1;

            foreach (var page in pages)
            {
                Program.SetStatus(String.Format("Updating...{0} - {1}/{2}", page, counter, pages.Count));

                // not sending them in parallel so facebook won't block us!
                _facebookService.UpdatePageEvents(page);
                _facebookService.UpdatePagePhotos(page);
                _facebookService.UpdatePageStatuses(page);

                await Task.Delay(1000);

                counter++;
            }
        }
    }
}