using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{ 
    [Authorize(Roles = "HR")]
    public class HrController : Controller
    {
        public IActionResult HrDashboard()
        {
            return View();
        }
    }
}