using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PetStore
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(FilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    // swallow everything and send the user home. less support tickets that way.
    public class HandleErrorAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            MvcApplication.Application_Error(context.Exception);
            var tempData = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
            if (tempData != null)
            {
                tempData.GetTempData(context.HttpContext)["Error"] = "Oops! Something went wrong: " + context.Exception.Message;
            }
            context.Result = new RedirectResult("/");
            context.ExceptionHandled = true;
        }
    }
}
