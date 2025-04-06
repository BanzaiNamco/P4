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
                    return View("../Admin/Add", course);
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
                return View("../Admin/Add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("../Admin/Add", data);
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

                return View("../Admin/Add", data);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        ModelState.AddModelError(string.Empty, "Course ID is required.");
        //        return RedirectToAction("Index");
        //    }
        //    var client = _httpClientFactory.CreateClient();
        //    var bearerToken = HttpContext.Session.GetString("JWToken");
        //    if (string.IsNullOrEmpty(bearerToken))
        //    {
        //        ModelState.AddModelError(string.Empty, "Authentication token is missing.");
        //        return RedirectToAction("Index", "Login");
        //    }
        //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        //    try
        //    {
        //        var response = await client.PostAsJsonAsync("https://localhost:8002/delete", id);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Error deleting course.");
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.RedirectWithError("Error connecting to the server.");
        //    }
        //}

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Save(CourseModel data)
        {
            if (!ModelState.IsValid)
                return View("../Admin/Add", data);
            if (string.IsNullOrEmpty(data.CourseName) || string.IsNullOrEmpty(data.CourseID))
            {
                ModelState.AddModelError(string.Empty, "Name and ID are required.");
                return View("../Admin/Add", data);
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

                return View("../Admin/Add", data);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }
        [HttpGet]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Enlist()
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
                _logger.LogInformation("Fetching passed courses for student: {StudentId}", User.Identity.Name);
                var response = await client.PostAsJsonAsync("https://localhost:8005/getPassedCourses", User.Identity.Name);
                if (!response.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var courseTaken = await response.Content.ReadFromJsonAsync<List<string>>();
                _logger.LogInformation("Fetching failed sections for student: {StudentId}", User.Identity.Name);
                var response2 = await client.PostAsJsonAsync("https://localhost:8005/getFailedSections", User.Identity.Name);
                if (!response2.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var sectionsFailed = await response2.Content.ReadFromJsonAsync<List<string>>();
                _logger.LogInformation("Fetching available courses for student: {StudentId}", User.Identity.Name);
                var response3 = await client.PostAsJsonAsync("https://localhost:8002/getAvailableCourses", courseTaken);
                if (!response3.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                _logger.LogInformation("Fetching available sections for student: {StudentId}", User.Identity.Name);
                var availableCourses = await response3.Content.ReadFromJsonAsync<List<string>>();
                var availableCourseWithSection = new List<CourseWithSectionModel>();
                foreach (var each in availableCourses)
                {
                    var data = new
                    {
                        CourseID = each,
                        sectionsFailed
                    };
                    var repeatingResponse = await client.PostAsJsonAsync("https://localhost:8003/getAvailableSections", data);
                    if (!repeatingResponse.IsSuccessStatusCode)
                    {
                        return this.RedirectWithError("Error fetching courses.");
                    }
                    var availableSections = await repeatingResponse.Content.ReadFromJsonAsync<List<SectionModel>>();
                    if (availableSections.Any())
                    {
                        availableCourseWithSection.Add(new CourseWithSectionModel
                        {
                            Course = new CourseModel
                            {
                                CourseID = each,
                                CourseName = ""
                            },
                            Sections = availableSections
                        });
                    }

                }
                _logger.LogInformation("Successfully fetched available courses and sections for student: {StudentId}", User.Identity.Name);
                if (TempData["ErrorMessage"] != null)
                {
                    ModelState.AddModelError(string.Empty, (string)TempData["ErrorMessage"]);
                }
                return View("../Student/Enlist", availableCourseWithSection);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");

            }
        }

        [HttpPost]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Enlist([FromBody] EnlistRequestModel data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (data == null)
            {
                return BadRequest("Invalid data.");
            }
            var courseID = data.CourseID;
            var sectionID = data.SectionID;
            _logger.LogInformation("Attempting to enlist in course: {CourseId}, section: {SectionId}", courseID, sectionID);
            if (string.IsNullOrEmpty(courseID) || string.IsNullOrEmpty(sectionID))
            {
                TempData["ErrorMessage"] = "Course ID and Section ID are required.";
                return RedirectToAction("Enlist");
            }
            var client = _httpClientFactory.CreateClient();
            var bearerToken = HttpContext.Session.GetString("JWToken");
            _logger.LogInformation("Fetching authentication token for student: {StudentId}", User.Identity.Name);
            if (string.IsNullOrEmpty(bearerToken))
            {
                ModelState.AddModelError(string.Empty, "Authentication token is missing.");
                return RedirectToAction("Index", "Login");
            }
            _logger.LogInformation("Setting authentication header for client.");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                var response2 = await client.PostAsJsonAsync("https://localhost:8003/incrementSlots", sectionID);
                if (!response2.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Error enrolling in course.";
                    return RedirectToAction("Enlist");
                }

                _logger.LogInformation("Attempting to enlist in course: {CourseId}, section: {SectionId}", courseID, sectionID);
                var body = new
                {
                    GradeID = "",
                    SectionID = sectionID,
                    CourseID = courseID,
                    StudentID = User.Identity.Name,
                    GradeValue = 5.0
                };
                var response = await client.PostAsJsonAsync("https://localhost:8005/add", body);
                _logger.LogInformation("Response from enlistment attempt: {StatusCode}", response.StatusCode);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Error enrolling in course.";
                    return RedirectToAction("Enlist");
                }
                _logger.LogInformation("Successfully enlisted in course: {CourseId}, section: {SectionId}", courseID, sectionID);

                return Ok(new { success = true, message = "Course enlisted successfully!" });
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");

            }
        }

        [HttpGet]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Enrolled()
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
                var response = await client.PostAsJsonAsync("https://localhost:8005/getEnrolledCourse", User.Identity.Name);
                if (!response.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var courseTaken = await response.Content.ReadFromJsonAsync<List<EnlistRequestModel>>();
                return View("../Student/Current", courseTaken);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");

            }
        }

        [HttpGet]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> All()
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
                var response = await client.GetAsync("https://localhost:8002/getAll");

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View("../Admin/Index", new List<CourseWithSectionModel>());

                }
                var resultString = await response.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<CourseModel>>(resultString);
                return View("../Student/All", courses);
            }
            catch (Exception ex)
            {
                return this.RedirectWithError("Error connecting to the server.");
            }
        }
    }
}
