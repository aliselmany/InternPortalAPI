using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography.X509Certificates;

namespace InternPortal.WebUI.Controllers
{
   
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Interns()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AdminDashboard(Guid internId)
        {
            ViewBag.InternId = internId;
            return View();
        }


    }

}