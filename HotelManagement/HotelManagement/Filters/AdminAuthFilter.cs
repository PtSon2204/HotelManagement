using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelManagement.Filters
{
    public class AdminAuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                context.Result = new RedirectResult("/Account/LoginRegister");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
