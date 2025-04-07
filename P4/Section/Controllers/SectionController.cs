using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Section.Data;
using Section.Models;

namespace Section.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SectionController> _logger;
        public SectionController(ApplicationDbContext dbContext, ILogger<SectionController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> add([FromBody] Section.Models.Section data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.CourseID) || string.IsNullOrEmpty(data.ProfID))
            {
                return BadRequest("Name and ID are required.");
            }
            var maxSectionID = _dbContext.Sections
                .AsEnumerable()
                .Select(s => int.TryParse(s.SectionID, out var id) ? id : 0)
                .DefaultIfEmpty(0)
                .Max();
            if (data.numStudents < 0)
            {
                return BadRequest("Number of students cannot be negative.");
            }
            if (data.maxStudents <= 0)
            {
                return BadRequest("Maximum number of students must be greater than zero.");
            }
            if (data.numStudents > data.maxStudents)
            {
                return BadRequest("Number of students cannot exceed maximum capacity.");
            }

            data.SectionID = (maxSectionID + 1).ToString();
            _dbContext.Sections.Add(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new { SectionID = data.SectionID });
        }

        [Authorize(Roles = "prof")]
        [HttpPost]
        public async Task<IActionResult> getByProf([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid course ID.");
            }
            var sections = await _dbContext.Sections
                .Where(s => s.ProfID == id)
                .ToListAsync();
            return Ok(sections);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> getByCourse([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid course ID.");
            }
            var sections = await _dbContext.Sections
                .Where(s => s.CourseID == id)
                .ToListAsync();
            return Ok(sections);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getAvailableSections([FromBody] CourseWithSection data)
        {
            if (data == null || string.IsNullOrEmpty(data.CourseID))
            {
                return BadRequest("Invalid data.");
            }
            var sections = await _dbContext.Sections
                .Where(s => s.CourseID == data.CourseID && s.numStudents < s.maxStudents && !data.sections.Contains(s.SectionID))
                .ToListAsync();
            return Ok(sections);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> incrementSlots([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid section ID.");
            }
            var section = await _dbContext.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound("Section not found.");
            }
            if (section.numStudents >= section.maxStudents)
            {
                return BadRequest("No available slots.");
            }
            section.numStudents++;
            await _dbContext.SaveChangesAsync();
            return Ok(section);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> decrementSlots([FromBody] string id)
        {
            _logger.LogInformation($"DecrementSlots method called with ID: {id}");
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid section ID.");
            }
            var section = await _dbContext.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound("Section not found.");
            }
            if (section.numStudents <= 0)
            {
                return BadRequest("No students to remove.");
            }
            _logger.LogInformation($"Current number of students: {section.numStudents}");
            section.numStudents--;
            await _dbContext.SaveChangesAsync();
            return Ok(section);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> delete([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid section ID.");
            }
            var section = await _dbContext.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound("Section not found.");
            }
            _dbContext.Sections.Remove(section);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Section deleted successfully." });
        }
    }
}
