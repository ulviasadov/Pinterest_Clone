using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestClone.Data;
using PinterestClone.Models;

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

    public IActionResult Index(string? search, string? filter)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        List<Pin> pins = new();
        List<User> users = new();

        if (!string.IsNullOrWhiteSpace(search))
        {
            pins = _context.Pins
                .Include(p => p.User)
                .Where(p => p.Title.Contains(search) || (p.Description != null && p.Description.Contains(search)))
                .OrderByDescending(p => p.Id)
                .ToList();

            if (string.IsNullOrEmpty(filter) || filter == "accounts")
            {
                users = _context.Users
                    .Where(u => u.Name.Contains(search) || u.Email.Contains(search))
                    .ToList();
            }

            ViewBag.Pins = pins;
            ViewBag.Users = users;
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.FollowingPins = null;
        }
        else if (userId != null)
        {
            var followingIds = _context.Follows
                .Where(f => f.FollowerId == userId.Value)
                .Select(f => f.FollowingId)
                .ToList();

            var followingPins = _context.Pins
                .Where(p => followingIds.Contains(p.UserId))
                .Include(p => p.User)
                .OrderByDescending(p => p.Id)
                .ToList();

            ViewBag.FollowingPins = followingPins;
            ViewBag.Pins = null;
            ViewBag.Users = null;
            ViewBag.Search = null;
            ViewBag.Filter = null;
        }
        else
        {
            ViewBag.FollowingPins = new List<Pin>();
            ViewBag.Pins = null;
            ViewBag.Users = null;
            ViewBag.Search = null;
            ViewBag.Filter = null;
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
        return View();
    }
}
