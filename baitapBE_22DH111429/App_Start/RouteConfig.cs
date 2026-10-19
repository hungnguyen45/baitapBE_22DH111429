
using System.Web.Mvc;
using System.Web.Routing;

namespace baitapBE_22DH111429
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                },
                namespaces: new[]
                {
                    "baitapBE_22DH111429.Controllers"
                }
            );
        }
    }
}
