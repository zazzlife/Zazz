using System;
using System.ServiceProcess;
using System.Timers;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.BackgroundServices
{
    public class TagStatisticsService : ServiceBase
    {
        private readonly Timer _timer = new Timer(1000 * 60 * 5);
        private readonly IUoW _uow;
        private readonly IStaticDataRepository _staticDataRepo;
        private readonly TagService _tagService;

        public TagStatisticsService()
        {

#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();
#endif

            //TODO: Move this initializations to an IoC container.
            _uow = new UoW();
            _staticDataRepo = new StaticDataRepository();
            _tagService = new TagService(_uow, _staticDataRepo);

            _timer.Elapsed += OnTimerElapsed;

            CanPauseAndContinue = true;
            CanStop = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _tagService.UpdateTagStatistics();
        }

        protected override void OnPause()
        {
            _timer.Stop();

            base.OnPause();
        }

        protected override void OnContinue()
        {
            _timer.Start();

            base.OnContinue();
        }

        protected override void OnStart(string[] args)
        {
            _timer.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            _timer.Stop();

            base.OnStop();
        }
    }
}