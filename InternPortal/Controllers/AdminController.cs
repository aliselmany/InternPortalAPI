using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{
   
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}