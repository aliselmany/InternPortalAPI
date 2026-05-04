using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{
    public class ApplicationsController : Controller
    {
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MyApplication()
        {
            return View();
        }
    }
}