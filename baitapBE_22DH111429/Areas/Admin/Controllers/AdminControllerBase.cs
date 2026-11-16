using System.Web.Mvc;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public abstract class AdminControllerBase : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((Session["UserRole"] as string) != "A")
            {
                filterContext.Result = new RedirectResult(Url.Action("Login", "Account", new { area = "" }));
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
