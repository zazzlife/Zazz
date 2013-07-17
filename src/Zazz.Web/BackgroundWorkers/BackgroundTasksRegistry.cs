using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using FluentScheduler;
using StructureMap;
using Zazz.Core.Interfaces;

namespace Zazz.Web.BackgroundWorkers
{
    public class BackgroundTasksRegistry : Registry
    {
        public BackgroundTasksRegistry()
        {
            //Register tasks here
            Schedule<CategoriesUpdaterTask>()
                .ToRunNow()
                .AndEvery(5)
                .Minutes();
        }

        public override ITask GetTaskInstance<T>()
        {
            return ObjectFactory.GetInstance<T>();
        }
    }
}