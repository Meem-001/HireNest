using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsApiController : ControllerBase
    {
        private readonly ApplicationRepository _appRepo;

        public ApplicationsApiController(ApplicationRepository appRepo)
        {
            _appRepo = appRepo;
        }

        // GET: api/applications/student/5
        [HttpGet("student/{studentId}")]
        public IActionResult GetByStudent(int studentId)
        {
            var applications = _appRepo.GetApplicationsByStudent(studentId);
            return Ok(applications);
        }

        // GET: api/applications/company/5
        [HttpGet("company/{companyId}")]
        public IActionResult GetByCompany(int companyId)
        {
            var applications = _appRepo.GetApplicationsForCompany(companyId);
            return Ok(applications);
        }
    }
}