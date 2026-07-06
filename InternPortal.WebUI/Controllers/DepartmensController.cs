using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InternPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    [Authorize(Roles = "SystemAdmin")]
    public class DepartmentsController : ControllerBase
    {
     
        public DepartmentsController()
        {
       
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDepartment([FromBody] object departmentDto)
        {
            return Ok(new { Message = "Yeni departman oluşturma endpointi hazır." });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDepartments()
        {
           
            return Ok(new { Message = "Departman listesi endpointi hazır." });
        }

        

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] object updateDto)
        {
         
            return Ok(new { Message = "Departman güncelleme endpointi hazır." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
          
            return Ok(new { Message = "Departman silme endpointi hazır." });
        }
    }
}