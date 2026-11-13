using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace PetStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(IEndpointRouteBuilder routes)
        {
            // old urls from the 2012 site, marketing still links to them
            routes.MapControllerRoute("legacy-pet", "pet.aspx", new { controller = "Pets", action = "Details" });
            routes.MapControllerRoute("legacy-shop", "shop/{categoryId?}", new { controller = "Pets", action = "Index" });

            routes.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
