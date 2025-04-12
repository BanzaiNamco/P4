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
        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (TempData["ErrorMessage"] != null)
            {
                ModelState.AddModelError(string.Empty, (string)TempData["ErrorMessage"]);
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
                    return View("Index", "Login");
                }

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                try
                {
                    var response = await client.PostAsJsonAsync("http://localhost:8003/getByProf", User.Identity.Name);
                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "Error fetching courses.");
                        return View("Prof");
                    }
                    var resultString = await response.Content.ReadAsStringAsync();
                    var sections = JsonConvert.DeserializeObject<List<SectionModel>>(resultString);
                    var courseWithSectionLIst = new List<CourseWithSectionModel>();

                    var uniqueCourseIDs = sections.Select(s => s.CourseID).Distinct().ToList();
                    foreach (var courseID in uniqueCourseIDs)
                    {
                        var course = new CourseModel
                        {
                            CourseID = courseID,
                            CourseName = ""
                        };
                        var sectionsForCourse = sections.Where(s => s.CourseID == courseID).ToList();
                        courseWithSectionLIst.Add(new CourseWithSectionModel
                        {
                            Course = course,
                            Sections = sectionsForCourse
                        });
                    }
                    return View("Prof", courseWithSectionLIst);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View("Prof");
                }
            }
            else if (role == "admin")
            {
                return View("Admin");
            }
            else if (role == "student")
            {
                return View("Student");
            }
            else
            {
                return RedirectToAction("Index, Login");
            }
        }
    }
}
