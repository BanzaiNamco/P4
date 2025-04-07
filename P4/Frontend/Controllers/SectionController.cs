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
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(data.ProfID) || string.IsNullOrEmpty(data.CourseID) ||
                data.maxStudents <= 0)
            {
                return BadRequest("All fields are required.");
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

                    var responseData = await response.Content.ReadFromJsonAsync<dynamic>();
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
                var data = new { bearerToken, id };
                var response = await client.PostAsJsonAsync("https://localhost:8005/deleteSection", data);
                if (!response.IsSuccessStatusCode)
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

        [HttpPost]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Drop([FromBody] string id)
        {
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
                var data = new
                {
                    IDWithBearerToken = new
                    {
                        bearerToken = bearerToken,
                        id = id
                    },
                    Name = User.Identity.Name
                };
                var response = await client.PostAsJsonAsync("https://localhost:8005/drop", data);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error dropping section.");
                }
                return Ok(new { success = true, message = "Section dropped successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error connecting to the server: {ex.Message}");
            }
        }
    }

}