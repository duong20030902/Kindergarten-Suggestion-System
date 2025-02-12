using Microsoft.AspNetCore.Mvc.Rendering;

namespace Group03_Kindergarten_Suggestion_System_Project.Helpers
{
    public static class ActiveMenuHelper
    {
        public static string IsActive(this IHtmlHelper htmlHelper, string controller, params string[] actions)
        {
            var routeData = htmlHelper.ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString();
            var currentAction = routeData.Values["action"]?.ToString();
            return (controller == currentController && actions.Contains(currentAction)) ? "active" : string.Empty;
        }

        public static string IsActiveParent(this IHtmlHelper htmlHelper, params string[] controllers)
        {
            var routeData = htmlHelper.ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString();
            return controllers.Contains(currentController) ? "active" : string.Empty;
            Console.WriteLine("OK");
        }
    }
}
