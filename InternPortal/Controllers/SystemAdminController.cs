using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace InternPortal.WebUI.Controllers 
{
    public class SystemAdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}