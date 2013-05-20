using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using StructureMap;

namespace Zazz.Web.DependencyResolution
{
    public class StructureMapFilterProvider : IFilterProvider
    {
        private readonly IContainer _container;

        public StructureMapFilterProvider(IContainer container)
        {
            _container = container;
        }

        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters();
            //    .Select(instance => new FilterInfo(instance, FilterScope.Controller));

            var actionFilters = actionDescriptor.GetFilters();
                //.Select(instance => new FilterInfo(instance, FilterScope.Action));

            var allFilters = controllerFilters.Concat(actionFilters);

            foreach (var filter in allFilters)
                _container.BuildUp(filter);

            return controllerFilters
                .Select(i => new FilterInfo(i, FilterScope.Controller))
                .Concat(actionFilters.Select(i => new FilterInfo(i, FilterScope.Action)));
        }
    }
}