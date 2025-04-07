using Frontend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class GradeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public GradeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "student")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return View("Index", "Login");
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8005/getPrevGrades", User.Identity.Name);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching grades.");
                    return View("Index");
                }
                var grades = await response.Content.ReadFromJsonAsync<List<GradeModel>>();
                return View("../Student/Previous", grades);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Internal server error: {ex.Message}");
                return View("Index", "Dashboard");
            }

        }

        [ValidateAntiForgeryToken]
        [Authorize(Roles = "prof")]
        [HttpPost]
        public async Task<IActionResult> Encode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return this.RedirectWithError("Section ID is required.");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return RedirectToAction("Index", "Login");
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8005/getStudents", id);
                if (!response.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error Fetching Students.");
                }
                var studentGrade = await response.Content.ReadFromJsonAsync<List<StudentGradeModel>>();
                return View("../Profs/Encode", studentGrade);

            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }


        [Authorize(Roles = "prof")]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] List<StudentGradeModel> data)
        {
            if (data == null)
            {
                return BadRequest("No data provided.");
            }
            if (!data.Any())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            if (data.Any(x => string.IsNullOrEmpty(x.StudentID) || x.GradeValue < 0 || x.GradeValue > 5))
            {
                return BadRequest("Invalid data provided.");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return RedirectToAction("Index", "Login");
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8005/save", data);
                if (!response.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error saving grades.");
                }
                return Ok(new { success = true, message = "Grades saved successfully." });
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }
    }
}