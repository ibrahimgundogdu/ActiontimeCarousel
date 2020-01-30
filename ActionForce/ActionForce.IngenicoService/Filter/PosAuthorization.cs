using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ActionForce.PosService
{
    public class PosAuthorization : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                string AuthorizationToken = actionContext.Request.Headers.Authorization.Parameter;

                if (ApiHelper.UserAuthentication(AuthorizationToken))
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(AuthorizationToken), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                }
            }

        }
    }
}