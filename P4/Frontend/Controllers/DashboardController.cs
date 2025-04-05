using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontend.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("AccessDenied", "Account"); // Redirect to a page that shows an error if role is missing
            }

            if (role == "prof")
            {
                return View("prof");
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
                return RedirectToAction("AccessDenied", "Account"); // Redirect if role is unrecognized
            }
        }
    }
}
