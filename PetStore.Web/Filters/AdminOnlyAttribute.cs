using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetStore.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isAdmin = filterContext.HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "1" && isAdmin != "True")
            {
                filterContext.Result = new RedirectResult("/Account/Login?returnUrl=" + filterContext.HttpContext.Request.Path);
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }

    // copy of AdminOnly for pages where you just need to be logged in
    public class MustLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.GetString("UserId") == null)
            {
                filterContext.Result = new RedirectResult("/Account/Login?returnUrl=" + filterContext.HttpContext.Request.Path);
            }
        }
    }
}
