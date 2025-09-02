using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PinterestClone.Controllers
{
    public class BaseController : Controller
    {
        protected void RestoreSessionFromCookies()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                var cookieUserId = Request.Cookies["UserId"];
                var cookieUserName = Request.Cookies["UserName"];
                if (!string.IsNullOrEmpty(cookieUserId) && int.TryParse(cookieUserId, out int parsedUserId))
                {
                    HttpContext.Session.SetInt32("UserId", parsedUserId);
                    if (!string.IsNullOrEmpty(cookieUserName))
                        HttpContext.Session.SetString("UserName", cookieUserName);
                }
            }
        }
    }
}

//using Microsoft.AspNetCore.Mvc;
//using PinterestClone.Data;

//namespace PinterestClone.Controllers
//{
//    public class BaseController : Controller
//    {
//        protected readonly ApplicationDbContext _context;

//        public BaseController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        protected void RestoreSessionFromCookies()
//        {
//            var sessionUserId = HttpContext.Session.GetInt32("UserId");
//            if (sessionUserId == null)
//            {
//                var cookieUserId = Request.Cookies["UserId"];
//                var cookieUserName = Request.Cookies["UserName"];
//                if (!string.IsNullOrEmpty(cookieUserId) && int.TryParse(cookieUserId, out int parsedUserId))
//                {
//                    HttpContext.Session.SetInt32("UserId", parsedUserId);
//                    if (!string.IsNullOrEmpty(cookieUserName))
//                        HttpContext.Session.SetString("UserName", cookieUserName);
//                }
//            }
//        }

//        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
//        {
//            base.OnActionExecuting(context);

//            var userId = HttpContext.Session.GetInt32("UserId");

//            if (userId.HasValue)
//            {
//                var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);

//                ViewBag.IsAdmin = user?.IsAdmin ?? false;
//                ViewBag.CurrentUserId = userId.Value;
//                ViewBag.CurrentUserName = user?.Name;
//                ViewBag.ProfileImagePath = user?.ProfileImagePath ?? "/images/PP.jpg";
//            }
//            else
//            {
//                ViewBag.IsAdmin = false;
//                ViewBag.CurrentUserId = null;
//                ViewBag.CurrentUserName = null;
//                ViewBag.ProfileImagePath = "/images/PP.jpg";
//            }
//        }
//    }
//}
