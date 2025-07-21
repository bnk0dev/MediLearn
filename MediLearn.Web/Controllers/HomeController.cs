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

        // Kullan�m ko�ullar� sayfas�n� d�ner
        public IActionResult TermsOfUse()
        {
            return View();
        }

        // Ana sayfa aksiyonu, burada �zel bir layout kullan�laca�� ViewData ile belirtilmi�
        public IActionResult Index()
        {
            ViewData["Layout"] = "_Layout";
            return View();
        }

        // Gizlilik politikas� sayfas�n� d�ner
        public IActionResult Privacy()
        {
            return View();
        }

        // Hata durumlar�nda �a�r�lan aksiyon
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
