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
                ComputedValue = _pluggableService.ComputeValue() + " CHANGED"
            };
            return View("Index.cshtml", model);
        }

        public IActionResult Privacy()
        {
            return View("Privacy.cshtml");
        }
    }
}