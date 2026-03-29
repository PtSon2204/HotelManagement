using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelManagement.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                context.Result = new UnauthorizedResult();
            }
            base.OnActionExecuting(context);
        }
    }
}
