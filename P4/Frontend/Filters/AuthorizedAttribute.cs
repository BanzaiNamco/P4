using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Frontend.Filters
{
    public class AuthorizedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the JWT token exists in session
            var token = context.HttpContext.Session.GetString("jwt");

            if (string.IsNullOrEmpty(token))
            {
                // Redirect to the login page if the token is not found
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
