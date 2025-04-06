using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Section.Data;

namespace Section.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public SectionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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

    }
}
