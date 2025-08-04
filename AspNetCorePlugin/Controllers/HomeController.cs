using Microsoft.AspNetCore.Mvc;
using AspNetCorePlugin.Services;
using AspNetCorePlugin.ViewModels;

namespace AspNetCorePlugin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPluggableService _pluggableService;

        public HomeController(IPluggableService pluggableService)
        {
            _pluggableService = pluggableService;
        }

        public IActionResult Index()
        {
            var model = new IndexViewModel
            {
                ComputedValue = _pluggableService.ComputeValue()
            };
            return View("/Views/Home/Index.cshtml", model);
        }

    }
}