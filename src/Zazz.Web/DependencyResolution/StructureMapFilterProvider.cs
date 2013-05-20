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
            var controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters()
                .Select(instance =>
                        {
                            _container.BuildUp(instance);
                            return new FilterInfo(instance, FilterScope.Controller);
                        });

            var actionFilters = actionDescriptor.GetFilters()
                .Select(instance =>
                        {
                            _container.BuildUp(instance);
                            return new FilterInfo(instance, FilterScope.Action);
                        });

            var allFilters = controllerFilters.Concat(actionFilters).ToList();
            return allFilters;
        }
    }
}