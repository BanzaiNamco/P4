using Frontend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class UserController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Add()
        {
            return View("add");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(UserModel data)
        {
            if (!ModelState.IsValid)
                return View("add", data);

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
                var response = await client.PostAsJsonAsync("https://localhost:8001/register", data);


                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Dashboard");
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
