using Frontend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace Frontend.Controllers
{
    [Route("login")]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;  // Inject IHttpClientFactory to make HTTP calls
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginController(ILogger<LoginController> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Display the Login view
        }

        // POST: /Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // If the form data is not valid, return the form with validation errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create an HttpClient instance using IHttpClientFactory
            var client = _httpClientFactory.CreateClient();

            // Prepare the payload (credentials)
            var payload = new
            {
                IDno = model.IDNumber,
                Password = model.Password
            };

            // Send a POST request to the API
            var response = await client.PostAsync("https://localhost:8001/login",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                // If the API returns an error (invalid credentials), show an error message
                ModelState.AddModelError("", "Invalid credentials. Please try again.");
                return View(model);
            }

            // If the login is successful, retrieve the token from the response
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
            string token = tokenResponse.token;
            // Store the JWT token in session or cookies (to authenticate future requests)
            await _httpContextAccessor.HttpContext.SignInAsync(
                "Cookies", // Use Cookies authentication scheme
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, model.IDNumber) }, "Cookies"))
            );

            //HttpContext.Session.SetString("jwt", token);

            // Redirect the user to the dashboard (or another page) after successful login
            return RedirectToAction("Index", "Dashboard");
        }

        // Error action (same as before, to handle any errors)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
