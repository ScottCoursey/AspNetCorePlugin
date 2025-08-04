using Microsoft.AspNetCore.Mvc.Razor;

namespace AspNetCorePlugin.Services
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        private readonly string _viewsRootPath;

        public CustomViewLocationExpander(string viewsRootPath)
        {
            _viewsRootPath = Path.GetFullPath(viewsRootPath);
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var expandedLocations = new List<string>();
            expandedLocations.Add(context.ViewName);
            if (!context.ViewName.StartsWith("/"))
            {
                foreach (var location in viewLocations)
                {
                    expandedLocations.Add(location);
                }
            }
            return expandedLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["customViewLocation"] = _viewsRootPath;
        }
    }
}