using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Frontend.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(IHttpClientFactory httpClientFactory, ILogger<DashboardController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (TempData["ErrorMessage"] != null)
            {
                ModelState.AddModelError(string.Empty, "Error connecting to the server.");
            }
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Index, Login");
            }

            if (role == "prof")
            {
                var client = _httpClientFactory.CreateClient();
                var bearerToken = HttpContext.Session.GetString("JWToken");

                if (string.IsNullOrEmpty(bearerToken))
                {
                    ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                    return View();
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                try
                {
                    var response = await client.PostAsJsonAsync("https://localhost:8003/getByProf", new { ProfID = User.Identity.Name });
                    _logger.LogInformation("Response: {response}", response);
                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "Error fetching courses.");
                        return View("prof");
                    }
                    var resultString = await response.Content.ReadAsStringAsync();
                    var sections = JsonConvert.DeserializeObject<List<SectionModel>>(resultString);
                    return View("prof", sections);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View("prof");
                }
            }
            else if (role == "admin")
            {
                return View("admin");
            }
            else if (role == "student")
            {
                return View("student");
            }
            else
            {
                return RedirectToAction("Index, Login");
            }
        }
    }
}
