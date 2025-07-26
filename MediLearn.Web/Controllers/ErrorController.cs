using Microsoft.AspNetCore.Mvc;

namespace MediLearn.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound"); // Views/Shared/NotFound.cshtml
                default:
                    return View("Error"); // Genel hata sayfası (isteğe bağlı)
            }
        }
    }

}
