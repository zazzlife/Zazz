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

        public TagStatisticsService()
        {

#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();
#endif
            _timer.Elapsed += OnTimerElapsed;

            CanPauseAndContinue = true;
            CanStop = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            using (var uow = new UoW())
            {
                var staticDataRepo = new StaticDataRepository();
                var tagService = new TagService(uow, staticDataRepo);
                tagService.UpdateTagStatistics();
            }
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