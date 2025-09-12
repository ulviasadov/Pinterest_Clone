using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PinterestClone.Models;
using PinterestClone.Data;

namespace PinterestClone.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(string search, string filter)
    {
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

        var pins = new List<Pin>();
        var users = new List<User>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (string.IsNullOrEmpty(filter) || filter == "pins")
            {
                pins = _context.Pins.Where(p => p.Title.Contains(search) || (p.Description != null && p.Description.Contains(search))).ToList();
            }
            if (string.IsNullOrEmpty(filter) || filter == "accounts")
            {
                users = _context.Users.Where(u => u.Name.Contains(search) || u.Email.Contains(search)).ToList();
            }
        }
        ViewBag.Pins = pins;
        ViewBag.Users = users;
        ViewBag.Search = search;
        ViewBag.Filter = filter;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
    return View();
    }
}
