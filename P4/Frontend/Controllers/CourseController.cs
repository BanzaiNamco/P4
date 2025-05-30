﻿using Frontend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Frontend.Controllers
{
    public class CourseController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CourseController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
                var response = await client.GetAsync("http://localhost:8002/getAll");

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching courses.");
                    return View("../Admin/Index", new List<CourseWithSectionModel>());

                }
                var resultString = await response.Content.ReadAsStringAsync();

                var courses = JsonConvert.DeserializeObject<List<CourseModel>>(resultString);
                var courseWithSectionLIst = new List<CourseWithSectionModel>();
                foreach (var course in courses)
                {
                    var sections = await client.PostAsJsonAsync("http://localhost:8003/getByCourse", course.CourseID);
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
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error fetching sections.");
                        return View("../Admin/Index", new List<CourseWithSectionModel>());
                    }
                }
                var url = "http://localhost:8001/getAllProf";

                var getProfResponse = await client.GetAsync(url);

                if (!getProfResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Error fetching professors.");
                    return View("../Admin/Index", new List<CourseWithSectionModel>());
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
                var response = await client.PostAsJsonAsync("http://localhost:8002/get", id);
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
                var response = await client.PostAsJsonAsync("http://localhost:8002/add", data);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Course is already added. You may edit it instead.");
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
        //        var response = await client.PostAsJsonAsync("http://localhost:8002/delete", id);
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
                var response = await client.PostAsJsonAsync("http://localhost:8002/save", data);
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
                var response = await client.PostAsJsonAsync("http://localhost:8005/getPassedCourses", User.Identity.Name);
                if (!response.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var courseTaken = await response.Content.ReadFromJsonAsync<List<string>>();
                var response2 = await client.PostAsJsonAsync("http://localhost:8005/getFailedSections", User.Identity.Name);
                if (!response2.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var sectionsFailed = await response2.Content.ReadFromJsonAsync<List<string>>();
                var response3 = await client.PostAsJsonAsync("http://localhost:8002/getAvailableCourses", courseTaken);
                if (!response3.IsSuccessStatusCode)
                {
                    return this.RedirectWithError("Error fetching courses.");
                }
                var availableCourses = await response3.Content.ReadFromJsonAsync<List<string>>();
                var availableCourseWithSection = new List<CourseWithSectionModel>();
                foreach (var each in availableCourses)
                {
                    var data = new
                    {
                        CourseID = each,
                        sectionsFailed
                    };
                    var repeatingResponse = await client.PostAsJsonAsync("http://localhost:8003/getAvailableSections", data);
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
     if (string.IsNullOrEmpty(courseID) || string.IsNullOrEmpty(sectionID))
     {
         TempData["ErrorMessage"] = "Course ID and Section ID are required.";
         return BadRequest("Course ID and Section ID are required.");
     }
     var client = _httpClientFactory.CreateClient();
     var bearerToken = HttpContext.Session.GetString("JWToken");
     if (string.IsNullOrEmpty(bearerToken))
     {
         ModelState.AddModelError(string.Empty, "Authentication token is missing.");
         return BadRequest("Authentication token is missing.");
     }
     client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
     try
     {
         var body = new
         {
             Grade = new
             {
                 GradeID = "",
                 SectionID = sectionID,
                 CourseID = courseID,
                 StudentID = User.Identity.Name,
                 GradeValue = 5.0
             },
             bearerToken = bearerToken
         };
         var response = await client.PostAsJsonAsync("http://localhost:8005/add", body);
         if (!response.IsSuccessStatusCode)
         {
             TempData["ErrorMessage"] = "Error enrolling in course.";
             return BadRequest("Error enrolling in course.");
         }

         return Ok(new { success = true, message = "Course enlisted successfully!" });
     }
     catch (Exception ex)
     {
         return BadRequest(ex.Message);
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
                var response = await client.PostAsJsonAsync("http://localhost:8005/getEnrolledCourse", User.Identity.Name);
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
                var response = await client.GetAsync("http://localhost:8002/getAll");

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
