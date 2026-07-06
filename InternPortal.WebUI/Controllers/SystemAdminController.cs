using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InternPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin")]
    public class SystemAdminController : ControllerBase
    {
        
        public SystemAdminController()
        {
         
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
       
            return Ok(new { Message = "Dashboard özet endpointi hazır." });
        }

        [HttpGet("calendar-events")]
        public async Task<IActionResult> GetCalendarEvents()
        {
         
            return Ok(new { Message = "Takvim verileri endpointi hazır." });
        }

        [HttpGet("global-logs")]
        public async Task<IActionResult> GetGlobalLogs()
        {
       
            return Ok(new { Message = "Sistem logları endpointi hazır." });
        }
    }
}