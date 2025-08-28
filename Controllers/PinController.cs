
using Microsoft.AspNetCore.Mvc;
using PinterestClone.Data;
using PinterestClone.Models;
using Microsoft.EntityFrameworkCore;

namespace PinterestClone.Controllers
{
    public class PinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PinController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // POST: /Pin/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(int pinId, int boardId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            // Check if board belongs to user
            var board = _context.Boards.FirstOrDefault(b => b.Id == boardId && b.UserId == userId.Value);
            if (board == null)
                return Json(new { success = false, message = "Invalid board." });

            // Check if already saved
            var alreadySaved = _context.PinBoards.Any(pb => pb.PinId == pinId && pb.BoardId == boardId);
            if (!alreadySaved)
            {
                _context.PinBoards.Add(new PinBoard { PinId = pinId, BoardId = boardId });
                _context.SaveChanges();

                // If this is the first pin in the board, set as cover image
                var pinCount = _context.PinBoards.Count(pb => pb.BoardId == boardId);
                if (pinCount == 1)
                {
                    var pin = _context.Pins.FirstOrDefault(p => p.Id == pinId);
                    if (pin != null)
                    {
                        board.CoverImagePath = pin.ImagePath;
                        _context.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "This pin already exists in this board." });
        }

// ...existing code...

        // GET: /Pin
        public async Task<IActionResult> Index(string? query)
        {
            var pinsQuery = _context.Pins
                .Include(p => p.User)
                .Include(p => p.PinLikes)
                .OrderByDescending(p => p.Id)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                pinsQuery = pinsQuery.Where(p => p.Title.Contains(query) || (p.Description != null && p.Description.Contains(query)));
            }
            var pins = await pinsQuery.ToListAsync();
            ViewBag.Query = query;
            return View(pins);
        }

        // GET: /Pin/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var boards = _context.Boards.Where(b => b.UserId == userId.Value).ToList();
            ViewBag.Boards = boards;
            return View();
        }

        // POST: /Pin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] Pin pin, [FromForm] IFormFile imageFile, [FromForm] int[] boardIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }


            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var sanitizedFileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + sanitizedFileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    pin.ImagePath = "/uploads/" + uniqueFileName;
                }

                pin.UserId = userId.Value;
                _context.Pins.Add(pin);
                await _context.SaveChangesAsync();

                if (boardIds.Length > 0)
                {
                    foreach (var boardId in boardIds)
                    {
                        var exists = _context.PinBoards.Any(pb => pb.PinId == pin.Id && pb.BoardId == boardId);
                        if (!exists)
                        {
                            var pinBoard = new PinBoard { PinId = pin.Id, BoardId = boardId };
                            _context.PinBoards.Add(pinBoard);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            var boards = _context.Boards.Where(b => b.UserId == userId.Value).ToList();
            ViewBag.Boards = boards;
            return View(pin);
        }

        public IActionResult Details(int id)
        {
            var pin = _context.Pins
                .Include(p => p.User)
                .Include(p => p.PinComments)
                    .ThenInclude(pc => pc.User)
                .FirstOrDefault(p => p.Id == id);
            if (pin == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var boards = _context.Boards.Where(b => b.UserId == userId.Value).ToList();
                ViewBag.Boards = boards;
            }
            else
            {
                ViewBag.Boards = null;
            }
            return View(pin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int pinId, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "Comment can not be empty.";
                return RedirectToAction("Details", new { id = pinId });
            }
            var comment = new PinComment
            {
                PinId = pinId,
                UserId = userId.Value,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.PinComments.Add(comment);
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = pinId });
        }

        // POST: /Pin/Like/5
        [HttpPost]
        public IActionResult Like(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "You must be logged in." });

            var alreadyLiked = _context.PinLikes.Any(pl => pl.PinId == id && pl.UserId == userId.Value);
            if (!alreadyLiked)
            {
                _context.PinLikes.Add(new PinLike { PinId = id, UserId = userId.Value });
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "You have already liked this pin." });
        }

        // POST: /Pin/Unlike/5
        [HttpPost]
        public IActionResult Unlike(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "You must be logged in." });

            var like = _context.PinLikes.FirstOrDefault(pl => pl.PinId == id && pl.UserId == userId.Value);
            if (like != null)
            {
                _context.PinLikes.Remove(like);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Like not found." });
        }

        // GET: /Pin/Explore
        public IActionResult Explore()
        {
            // Popüler pinler (en çok beğenilen ilk 10)
            var popularPins = _context.Pins
                .Include(p => p.User)
                .Include(p => p.PinLikes)
                .OrderByDescending(p => p.PinLikes.Count)
                .Take(10)
                .ToList();
            // Yeni eklenen pinler (son 10)
            var newPins = _context.Pins
                .Include(p => p.User)
                .OrderByDescending(p => p.Id)
                .Take(10)
                .ToList();
            // Kullanıcıya öneri (kendi pinleri hariç rastgele 10 pin)
            var userId = HttpContext.Session.GetInt32("UserId");
            List<Pin> recommendedPins = new();
            if (userId != null)
            {
                recommendedPins = _context.Pins
                    .Include(p => p.User)
                    .Where(p => p.UserId != userId.Value)
                    .OrderBy(r => Guid.NewGuid())
                    .Take(10)
                    .ToList();
            }
            ViewBag.PopularPins = popularPins;
            ViewBag.NewPins = newPins;
            ViewBag.RecommendedPins = recommendedPins;
            return View();
        }
    }
}
