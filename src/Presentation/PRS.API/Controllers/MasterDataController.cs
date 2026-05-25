using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRS.Infrastructure.Data; // Updated to match your DbContext namespace

namespace PRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/masterdata/employmentstatus
        [HttpGet("employmentstatus")]
        public async Task<IActionResult> GetEmploymentStatuses()
        {
            // Using your exact DbSet name: EmploymentStatuses
            var statuses = await _context.EmploymentStatuses 
                // Note: Make sure these property names (Employment_Status_ID, etc.) 
                // exactly match the properties inside your EmploymentStatus.cs model class!
                .Select(e => new { e.Employment_Status_ID, e.Employment_Status, e.Employment_Status_Code })
                .ToListAsync();
            return Ok(statuses);
        }

        // GET: api/masterdata/grades
        // GET: api/masterdata/grades
        [HttpGet("grades")]
        public async Task<IActionResult> GetGrades()
        {
            var grades = await _context.Grades
                .Where(g => g.Status == "Active") // <-- Changed to "Active"
                .Select(g => new { g.Grade_ID, g.Rank, g.Rank_Code })
                .ToListAsync();
            return Ok(grades);
        }

        // GET: api/masterdata/lineofservice
        [HttpGet("lineofservice")]
        public async Task<IActionResult> GetLineOfServices()
        {
            // Using your exact DbSet name: LinesOfService
            var los = await _context.LinesOfService
                .Select(l => new { l.LOS_ID, LineOfService = l.Line_Of_Service, l.LOS_CODE })
                .ToListAsync();
            return Ok(los);
        }

        // GET: api/masterdata/officelocations/{countryCode}
        // GET: api/masterdata/officelocations/{countryCode}
        [HttpGet("officelocations/{countryCode}")]
        public async Task<IActionResult> GetOfficeLocations(string countryCode)
        {
            var locations = await _context.WorkOffices
                .Where(w => w.Country_Code == countryCode && w.WOL_Status == "Active") // <-- Changed to "Active"
                .Select(w => new { w.Work_Office_ID, w.Work_Office_Code, w.Work_Office_Description })
                .ToListAsync();
            return Ok(locations);
        }
    }
}