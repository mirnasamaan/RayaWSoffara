using System.Web;
using System.Web.Mvc;

public class CustomAuthorize : AuthorizeAttribute
{
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        // If they are authorized, handle accordingly
        if (this.AuthorizeCore(filterContext.HttpContext))
        {
            base.OnAuthorization(filterContext);
        }
        else
        {
            var data = filterContext.HttpContext.Request.Url;
            // Otherwise redirect to your specific authorized area
            filterContext.Result = new RedirectResult("/Admin/Signin?RedirectUrl=" + data);
            //filterContext.Result = new HttpUnauthorizedResult();
            filterContext.Controller.TempData.Add("RedirectReason", "You are not authorized to access this page.");
            //filterContext.Result = new RedirectResult("~/Admin/Signin");
        }
    }
}