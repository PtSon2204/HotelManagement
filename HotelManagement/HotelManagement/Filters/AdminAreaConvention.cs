using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HotelManagement.Filters
{
    public class AdminAreaConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.RouteValues.TryGetValue("area", out var area) && area == "Admin")
            {
                controller.Filters.Add(new AdminAuthFilter());
            }
        }
    }
}
