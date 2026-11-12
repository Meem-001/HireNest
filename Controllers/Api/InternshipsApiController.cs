using InternshipManagementSystem.Data;
using InternshipManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagementSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternshipsApiController : ControllerBase
    {
        private readonly InternshipRepository _intRepo;

        public InternshipsApiController(InternshipRepository intRepo)
        {
            _intRepo = intRepo;
        }

        // GET: api/internships
        [HttpGet]
        public IActionResult GetAll()
        {
            var internships = _intRepo.GetAllInternships();
            return Ok(internships);
        }

        // GET: api/internships/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var internship = _intRepo.GetInternshipById(id);
            if (internship == null)
                return NotFound(new { message = "Internship not found" });

            return Ok(internship);
        }

        // GET: api/internships/company/5
        [HttpGet("company/{companyId}")]
        public IActionResult GetByCompany(int companyId)
        {
            var internships = _intRepo.GetInternshipsByCompanyId(companyId);
            return Ok(internships);
        }
    }
}