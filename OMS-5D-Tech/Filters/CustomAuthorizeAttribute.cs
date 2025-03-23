using System.Web.Mvc;  // Chắc chắn chỉ dùng Mvc

namespace OMS_5D_Tech.Filters
{
    public class CustomAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { httpStatus = 403, mess = "Bạn không có quyền truy cập!" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { httpStatus = 401, mess = "Vui lòng đăng nhập trước!" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}
