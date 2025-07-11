using System.Diagnostics;
using MediLearn.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediLearn.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult TermsOfUse()
        {
            return View();
        }

        public IActionResult Index()
        {
            ViewData["Layout"] = "_Layout";
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
