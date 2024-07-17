
using Microsoft.AspNetCore.Mvc;

namespace GamesWebSocket.Controllers
{
    public class ErrorPageController : Controller
    {
        public IActionResult Index()
        {
            return View("NotFound");
        }
    }
}
