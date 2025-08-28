using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PinterestClone.Models;

namespace PinterestClone.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
    // If there is no session, read UserId and UserName from cookie
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            var userIdCookie = Request.Cookies["UserId"];
            var userNameCookie = Request.Cookies["UserName"];
            if (!string.IsNullOrEmpty(userIdCookie) && !string.IsNullOrEmpty(userNameCookie))
            {
                if (int.TryParse(userIdCookie, out int userId))
                {
                    HttpContext.Session.SetInt32("UserId", userId);
                    HttpContext.Session.SetString("UserName", userNameCookie);
                }
            }
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
    return View(new PinterestClone.ViewModels.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
