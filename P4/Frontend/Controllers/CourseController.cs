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
                return RedirectToAction("Index", "Login");
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response = await client.GetAsync("https://localhost:8002/getAll");

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View("../Admin/Index", new List<CourseWithSectionModel>());

                }
                var resultString = await response.Content.ReadAsStringAsync();

                var courses = JsonConvert.DeserializeObject<List<CourseModel>>(resultString);
                var courseWithSectionLIst = new List<CourseWithSectionModel>();
                // for each of course
                foreach (var course in courses)
                {
                    var sections = await client.PostAsJsonAsync("https://localhost:8003/getByCourse", course.CourseID);
                    if (sections.IsSuccessStatusCode)
                    {
                        var resultString2 = await sections.Content.ReadAsStringAsync();
                        var sectionsList = JsonConvert.DeserializeObject<List<SectionModel>>(resultString2);
                        courseWithSectionLIst.Add(new CourseWithSectionModel
                        {
                            Course = course,
                            Sections = sectionsList
                        });
                    }
                }
                var url = "https://localhost:8001/getAllProf"; // URL without any query parameters

                var getProfResponse = await client.GetAsync(url); // Use GetAsync for a GET request

                if (!getProfResponse.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"External API error: {errorContent}");
                }
                var allProf = await getProfResponse.Content.ReadFromJsonAsync<List<string>>();
                ViewBag.allProf = allProf;
                return View("../Admin/Index", courseWithSectionLIst);
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
            return View("../admin/Add", new CourseModel());
        }


        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id)
        {
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
                var response = await client.PostAsJsonAsync("https://localhost:8002/get", id);
                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseModel>(resultString);
                    return View("../admin/Add", course);
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
                return View("../admin/Add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("../admin/Add", data);
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
                var response = await client.PostAsJsonAsync("https://localhost:8002/add", data);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Course already added. Edit instead.");
                }

                return View("../admin/Add", data);
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
                return RedirectToAction("Index", "Login");
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
                return View("../admin/Add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("../admin/Add", data);
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
                var response = await client.PostAsJsonAsync("https://localhost:8002/save", data);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error editing user.");
                }

                return View("../admin/Add", data);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }
    }
}
