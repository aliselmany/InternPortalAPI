using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{ 
    public class HrController : Controller
    {
        [HttpGet]
        public IActionResult HrDashboard()
        {
            return View("~/Views/Hr/HrDashboard.cshtml");
        }
    }
}