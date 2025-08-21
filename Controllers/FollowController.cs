using Microsoft.AspNetCore.Mvc;
using PinterestClone.Data;
using PinterestClone.Models;

namespace PinterestClone.Controllers
{
    public class FollowController : Controller
    {
    private readonly ApplicationDbContext _context;

    public FollowController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Follow(int userId)
        {
            var currentUserId = 0;
            if (currentUserId == userId) return BadRequest();

            var alreadyFollowing = _context.Follows
                .Any(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            if (!alreadyFollowing)
            {
                _context.Follows.Add(new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = userId,
                    FollowedAt = DateTime.Now
                });
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult Unfollow(int userId)
        {
            var currentUserId = 0;
            var follow = _context.Follows
                .FirstOrDefault(f => f.FollowerId == currentUserId && f.FollowingId == userId);
            if (follow != null)
            {
                _context.Follows.Remove(follow);
                _context.SaveChanges();
            }
            return Ok();
        }

        public IActionResult Followers(int userId)
        {
            var followerIds = _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => f.FollowerId)
                .ToList();
            var followers = _context.Users
                .Where(u => followerIds.Contains(u.Id))
                .ToList();
            return View(followers);
        }

        public IActionResult Following(int userId)
        {
            var followingIds = _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToList();
            var following = _context.Users
                .Where(u => followingIds.Contains(u.Id))
                .ToList();
            return View(following);
        }
    }
}