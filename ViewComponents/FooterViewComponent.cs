using Microsoft.AspNetCore.Mvc;

namespace PinterestClone.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
