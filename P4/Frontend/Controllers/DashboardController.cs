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
                return RedirectToAction("Index, Login");
            }
        }
    }
}
