using Frontend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Frontend.Controllers
{
    public class CourseController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CourseController> _logger;
        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return View(); // or return RedirectToAction("Login") if you want to redirect
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.GetAsync("https://localhost:8002/getAll");

                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();

                    var courses = JsonConvert.DeserializeObject<List<CourseModel>>(resultString);

                    return View("Index", courses); // Pass to a strongly-typed view
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Add()
        {
            return View("add", new CourseModel());
        }


        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id)
        {
            _logger.LogInformation("Editing course with ID: {id}", id);
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return RedirectToAction("Index");
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8002/get", id);
                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseModel>(resultString);
                    _logger.LogInformation("Fetched course: {course}", course);
                    return View("add", course);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");

            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(CourseModel data)
        {
            if (!ModelState.IsValid)
                return View("add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("add", data);
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return View("add", data);
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8002/add", data);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error adding user.");
                }

                return View("add", data);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError(string.Empty, "Course ID is required.");
                return RedirectToAction("Index");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return RedirectToAction("Index");
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8002/delete", id);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error deleting course.");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Save(CourseModel data)
        {
            if (!ModelState.IsValid)
                return View("add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("add", data);
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return View("add", data);
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8002/save", data);
                _logger.LogInformation("response: {response}", response);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error adding user.");
                }

                return View("add", data);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }
    }
}
