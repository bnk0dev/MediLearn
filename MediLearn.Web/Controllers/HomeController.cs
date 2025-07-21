using MediLearn.Web.Models;
using MediLearn.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace MediLearn.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<SharedResource> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        // Kullaným koþullarý sayfasýný döner
        public IActionResult TermsOfUse()
        {
            return View();
        }

        // Ana sayfa aksiyonu, burada özel bir layout kullanýlacaðý ViewData ile belirtilmiþ
        public IActionResult Index()
        {
            ViewData["Layout"] = "_Layout";
            return View();
        }

        // Gizlilik politikasý sayfasýný döner
        public IActionResult Privacy()
        {
            return View();
        }

        // Hata durumlarýnda çaðrýlan aksiyon
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
