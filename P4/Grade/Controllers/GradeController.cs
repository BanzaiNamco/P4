using Grade.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grade.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public GradeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getPassedCourses([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid student ID.");
            }
            var courses = await _dbContext.Grades
                .Where(g => g.StudentID == id && g.GradeValue >= 1.0)
                .Select(g => g.CourseID)
                .Distinct()
                .ToListAsync();
            return Ok(courses);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getFailedSections([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid student ID.");
            }
            var sections = await _dbContext.Grades
                .Where(g => g.StudentID == id && g.GradeValue < 1.0)
                .Select(g => g.SectionID)
                .Distinct()
                .ToListAsync();
            return Ok(sections);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> add([FromBody] Grade.Models.Grade data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.StudentID) || string.IsNullOrEmpty(data.CourseID) || string.IsNullOrEmpty(data.SectionID))
            {
                return BadRequest("Student ID, Course ID, and Section ID are required.");
            }
            var maxGradeID = await _dbContext.Grades
                .Where(g => g.StudentID == data.StudentID)
                .Select(g => g.GradeID)
                .MaxAsync() ?? "0";

            var newGradeID = (int.Parse(maxGradeID) + 1).ToString();
            data.GradeID = newGradeID;
            _dbContext.Grades.Add(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Grade added successfully." });

        }
    }
}
