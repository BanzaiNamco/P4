using Course.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Course.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public CourseController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> add([FromBody] Course.Models.Course data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                return BadRequest("Name and ID are required.");
            }
            var existingCourse = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.CourseID == data.CourseID || c.CourseName == data.CourseName);
            if (existingCourse != null)
            {
                return Conflict("Course with the same ID or Name already exists.");
            }
            _dbContext.Courses.Add(data);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Course added successfully." });
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var courses = await _dbContext.Courses.ToListAsync();
            return Ok(courses);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> delete([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid course ID.");
            }

            var course = await _dbContext.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Course deleted successfully." });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> get([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid course ID.");
            }
            var course = await _dbContext.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound("Course not found.");
            }
            return Ok(course);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> save([FromBody] Course.Models.Course data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                return BadRequest("Name and ID are required.");
            }
            var existingCourse = await _dbContext.Courses.FindAsync(data.CourseID);
            if (existingCourse == null)
            {
                return NotFound("Course not found.");
            }
            existingCourse.CourseName = data.CourseName;
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Course updated successfully." });
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> getAvailableCourses([FromBody] List<String> data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            var courses = await _dbContext.Courses
                .Where(c => !data.Contains(c.CourseID))
                .Select(c => c.CourseID)
                .ToListAsync();
            return Ok(courses);
        }



    }


}
