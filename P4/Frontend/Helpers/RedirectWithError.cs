using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontend.Helpers
{
    public static class RedirectHelper
    {
        public static IActionResult RedirectWithError(this Controller controller, string errorMessage)
        {
            var role = controller.User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
            {
                return controller.RedirectToAction("Index", "Login");
            }
            if (role == "prof" || role == "admin" || role == "student")
            {
                controller.TempData["ErrorMessage"] = errorMessage;
                return controller.RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return controller.RedirectToAction("Index", "Login");
            }
        }
    }
}
