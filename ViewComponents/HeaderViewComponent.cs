using Microsoft.AspNetCore.Mvc;

namespace PinterestClone.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            var profileImage = HttpContext.Session.GetString("ProfileImagePath")
                               ?? "/images/PP.jpg";

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.ProfileImagePath = profileImage;

            return View();
        }
    }
}
