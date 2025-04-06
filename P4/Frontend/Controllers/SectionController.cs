using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class SectionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public SectionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
                return Unauthorized("Authentication token is missing."); // Return Unauthorized
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
                    return StatusCode((int)response.StatusCode, $"External API error: {errorContent}"); // Return appropriate status code and error message
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
