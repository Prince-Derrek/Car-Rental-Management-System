using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRMS_UI.Filters
{
    public class AuthorizeRoleFilter : IActionFilter
    {
        private readonly string _requiredRole;

        public AuthorizeRoleFilter(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var token = context.HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userRole) ||
                !userRole.Equals(_requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
            
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}