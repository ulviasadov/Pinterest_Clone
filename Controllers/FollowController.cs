using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Follow(int userId)
        {

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                return BadRequest("User not found.");

            int currentId = (int)currentUserId;
            if (currentId == userId) return BadRequest();

            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentId && f.FollowingId == userId);

            if (!alreadyFollowing)
            {
                _context.Follows.Add(new Follow
                {
                    FollowerId = currentId,
                    FollowingId = userId,
                    FollowedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(int userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            int currentId = (int)currentUserId;

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentId && f.FollowingId == userId);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> Followers(int userId)
        {
            var followers = await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == userId)
                .Select(f => f.Follower)
                .ToListAsync();

            return View(followers);
        }

        public async Task<IActionResult> Following(int userId)
        {
            var following = await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowerId == userId)
                .Select(f => f.Following)
                .ToListAsync();

            return View(following);
        }
    }
}