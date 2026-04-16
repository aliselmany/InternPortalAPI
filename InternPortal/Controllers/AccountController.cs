using Microsoft.AspNetCore.Mvc;

namespace InternPortal.WebUI.Controllers
{
    public class AccountController : Controller
    {
    
        [HttpGet]
        public IActionResult Login()
        {
      
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
         
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            
            return RedirectToAction("Login");
        }
    }
}