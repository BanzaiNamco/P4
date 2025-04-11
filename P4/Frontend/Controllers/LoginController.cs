using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            if (TempData["ErrorMessage"] != null)
            {
                ModelState.AddModelError(string.Empty, (string)TempData["ErrorMessage"]);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel data)
        {
            if (!ModelState.IsValid)
                return View("Index", data);

            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.PostAsJsonAsync("https://localhost:8001/login", data);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("JWToken", result);
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid credentials.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error connecting to the server.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            return RedirectToAction("Index", "Login");
        }
    }
}
