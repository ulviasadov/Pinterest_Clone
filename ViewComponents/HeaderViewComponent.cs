using Microsoft.AspNetCore.Mvc;
using PinterestClone.Data;

namespace PinterestClone.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public HeaderViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            string profileImage = HttpContext.Session.GetString("ProfileImagePath") ?? "/images/PP.jpg";
            bool isAdmin = false;

            if (userId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
                if (user != null)
                {
                    isAdmin = user.IsAdmin;
                }
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.ProfileImagePath = profileImage;

            return View();
        }
    }
}
