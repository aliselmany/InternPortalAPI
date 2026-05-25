using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{
    public class StaffController : Controller
    {
        [HttpGet]
        public IActionResult StaffDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StaffDashboardIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StaffProfile()
        {
            return View();
        }


    }
}
