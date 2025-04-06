using Grade.Data;
using Grade.Models;
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
                .Select(g => g.GradeID)
                .MaxAsync() ?? "0";

            var newGradeID = (int.Parse(maxGradeID) + 1).ToString();
            data.GradeID = newGradeID;
            _dbContext.Grades.Add(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Grade added successfully." });

        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getPrevGrades([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid student ID.");
            }
            var grades = await _dbContext.Grades
                .Where(g => g.StudentID == id && g.GradeValue != 5)
                .ToListAsync();
            return Ok(grades);
        }
        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getEnrolledCourse([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid student ID.");
            }
            var grades = await _dbContext.Grades
                .Where(g => g.StudentID == id && g.GradeValue == 5)
                .Select(g => new { g.CourseID, g.SectionID })
                .ToListAsync();
            return Ok(grades);
        }


        [Authorize(Roles = "prof")]
        [HttpPost]
        public async Task<IActionResult> getStudents([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid section ID.");
            }
            var students = await _dbContext.Grades
                .Where(g => g.SectionID == id)
                .Select(g => new { g.StudentID, g.GradeValue })
                .Distinct()
                .ToListAsync();

            return Ok(students);
        }

        [Authorize(Roles = "prof")]
        [HttpPost]
        public async Task<IActionResult> save([FromBody] List<StudentGrade> data)
        {
            if (data == null || !data.Any())
            {
                return BadRequest("No data provided.");
            }
            if (data.Any(x => string.IsNullOrEmpty(x.StudentID) || x.GradeValue < 0 || x.GradeValue > 5))
            {
                return BadRequest("Invalid data provided.");
            }
            foreach (var item in data)
            {
                var grade = await _dbContext.Grades
                    .FirstOrDefaultAsync(g => g.StudentID == item.StudentID);
                if (grade != null)
                {
                    grade.GradeValue = item.GradeValue;
                    _dbContext.Grades.Update(grade);
                }
            }
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Grades updated successfully." });

        }

    }
}
