using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class SectionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SectionController> _logger;
        public SectionController(IHttpClientFactory httpClientFactory, ILogger<SectionController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add([FromBody] SectionModel data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return BadRequest with validation errors
            }
            if (string.IsNullOrEmpty(data.ProfID) || string.IsNullOrEmpty(data.CourseID) ||
                data.maxStudents <= 0)
            {
                return BadRequest("All fields are required."); // Return BadRequest with a message
            }

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

                var response = await client.PostAsJsonAsync("https://localhost:8003/add", data);
                if (response.IsSuccessStatusCode)
                {

                    var responseData = await response.Content.ReadFromJsonAsync<dynamic>(); // Deserialize to a dynamic object
                    return Ok(responseData);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorContent);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete([FromBody] string id)
        {
            _logger.LogInformation($"Delete method called with ID: {id}");
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid section ID.");
            }
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

                var response = await client.PostAsJsonAsync("https://localhost:8003/delete", id);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error deleting section.");
                }
                var response2 = await client.PostAsJsonAsync("https://localhost:8005/deleteSection", id);
                if (!response2.IsSuccessStatusCode)
                {
                    return BadRequest("Error deleting enrolled students.");
                }
                return Ok(new { success = true, message = "Section deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
        }
    }
}
