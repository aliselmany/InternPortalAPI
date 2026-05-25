using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 

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

        [HttpGet]
        public IActionResult InternDashboard()
        {
          return View();
        }

        [HttpGet]
        public IActionResult MentorSelection()
        {
            return View();
        }
    }  
}