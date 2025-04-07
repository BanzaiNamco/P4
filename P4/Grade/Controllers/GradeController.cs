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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GradeController> _logger;
        public GradeController(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<GradeController> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
        public async Task<IActionResult> add([FromBody] GradeWithBearerToken data)
        {
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            if (string.IsNullOrEmpty(data.Grade.StudentID) || string.IsNullOrEmpty(data.Grade.CourseID) || string.IsNullOrEmpty(data.Grade.SectionID))
            {
                return BadRequest("Student ID, Course ID, and Section ID are required.");
            }
            if (string.IsNullOrEmpty(data.BearerToken))
            {
                return BadRequest("Authentication token is missing.");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = data.BearerToken;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8003/incrementSlots", data.Grade.SectionID);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to increment slots.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
            var maxGradeID = await _dbContext.Grades
                .Select(g => g.GradeID)
                .MaxAsync() ?? "0";

            var newGradeID = (int.Parse(maxGradeID) + 1).ToString();
            data.Grade.GradeID = newGradeID;
            _dbContext.Grades.Add(data.Grade);
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

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> deleteSection([FromBody] IDWithBearerToken data)
        {
            _logger.LogInformation($"DeleteSection method called with ID: {data.ID}");
            if (string.IsNullOrEmpty(data.ID))
            {
                return BadRequest("Invalid section ID.");
            }
            if (string.IsNullOrEmpty(data.BearerToken))
            {
                return BadRequest("Authentication token is missing.");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = data.BearerToken;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            _logger.LogInformation($"Bearer token: {bearerToken}");
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8003/delete", data.ID);
                _logger.LogInformation($"Response status code: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to increment slots.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
            var grades = await _dbContext.Grades
                .Where(g => g.SectionID == data.ID)
                .ToListAsync();
            if (grades.Any())
            {
                _dbContext.Grades.RemoveRange(grades);
                await _dbContext.SaveChangesAsync();
            }
            return Ok(new { message = "Section deleted successfully." });
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> drop([FromBody] IDWithNameWithBearerToken data)
        {
            _logger.LogInformation($"Drop method called with ID: {data.IDWithBearerToken.ID}");
            _logger.LogInformation($"User name: {data.Name}");
            _logger.LogInformation($"Bearer token: {data.IDWithBearerToken.BearerToken}");
            _logger.LogInformation($"Data to be sent: {data.IDWithBearerToken.ID}");
            if (string.IsNullOrEmpty(data.IDWithBearerToken.ID))
            {
                return BadRequest("Invalid section ID.");
            }
            if (string.IsNullOrEmpty(data.IDWithBearerToken.BearerToken))
            {
                return BadRequest("Authentication token is missing.");
            }
            if (string.IsNullOrEmpty(data.Name))
            {
                return BadRequest("User name is required.");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = data.IDWithBearerToken.BearerToken;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            _logger.LogInformation($"Bearer token: {bearerToken}");
            try
            {
                _logger.LogInformation($"Data to be sent: {data.IDWithBearerToken.ID}");
                var response = await client.PostAsJsonAsync("https://localhost:8003/decrementSlots", data.IDWithBearerToken.ID);
                _logger.LogInformation($"Response status code: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error decrement slots.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
            var grades = await _dbContext.Grades
                .Where(g => g.StudentID == data.Name)
                .ToListAsync();
            if (grades.Any())
            {
                _dbContext.Grades.RemoveRange(grades);
                await _dbContext.SaveChangesAsync();
            }
            return Ok(new { message = "Student dropped successfully." });
        }
    }
}
