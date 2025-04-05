using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace OMS_5D_Tech.Filters
{
    public class CustomAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var user = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized,
                    new { httpStatus = 401, mess = "Vui lòng đăng nhập trước!" });
                return;
            }

            var isVerify = user.Claims.FirstOrDefault(c => c.Type == "IsVerify")?.Value;

            if (isVerify == "False")
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized,
                    new { httpStatus = 401, mess = "Tài khoản của bạn chưa được kích hoạt. Vui lòng xác thực email!" });
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                var userRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                if (!userRoles.Intersect(Roles.Split(',')).Any())
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden,
                        new { httpStatus = 403, mess = "Bạn không có quyền truy cập!" });
                    return;
                }
            }

            base.OnAuthorization(actionContext);
        }
    }
}
