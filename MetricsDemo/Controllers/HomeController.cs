using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MetricsDemo.Models;
using App.Metrics;
using App.Metrics.Counter;

namespace MetricsDemo.Controllers
{
    public class HomeController : Controller
    {
        private IMetricsRoot _metrics;

        public HomeController(IMetricsRoot metricsRoot)
        {
            _metrics = metricsRoot;
        }

        public IActionResult Index()
        {
            IncrementCustom("home");

            return View();
        }

        public IActionResult About()
        {
            IncrementCustom("about");

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        private void IncrementCustom(string str)
        {
            _metrics.Measure.Counter.Increment(new CounterOptions
            {
                Name = "custom",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("val", str)
            });
        }

        public IActionResult Contact()
        {
            IncrementCustom("contact");

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
