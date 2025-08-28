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
