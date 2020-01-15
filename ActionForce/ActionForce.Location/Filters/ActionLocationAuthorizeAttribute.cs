using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ActionForce.Entity;

namespace ActionForce.Location
{
    public class ActionLocationAuthorizeAttribute : AuthorizeAttribute
    {
        public bool CheckPermission(HttpContextBase httpContext)
        {
            if (httpContext.User != null && httpContext.User.Identity is FormsIdentity identity)
            {
                var authModel = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);
                var controller = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
                var action = httpContext.Request.RequestContext.RouteData.Values["action"].ToString();
                using (var db = new ActionTimeEntities())
                {
                    var roleGroupPermissions = db.RoleGroupPermissions.Include("Permission").Where(x => x.RoleGroupID == authModel.CurrentUser.CurrentRoleGroup.ID);
                    return roleGroupPermissions.Any(x => x.Permission.Controller == controller && x.Permission.Action == action && x.IsNavigate == true);
                }
            }
            //not authenticated
            return false;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //return base.AuthorizeCore(httpContext);
            return CheckPermission(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // The user is not authenticated
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                var authModel = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>((filterContext.HttpContext.User.Identity as FormsIdentity).Ticket.UserData);
                var layoutModel = new LayoutControlModel
                {
                    Authentication = authModel
                };
                // The user is not in any of the listed roles => 
                // show the unauthorized view
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Error/Unauthorized.cshtml",
                    ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                    {
                        Model = layoutModel
                    }
                };
                //base.HandleUnauthorizedRequest(filterContext);
            }
        }

    }
}