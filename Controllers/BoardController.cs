using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestClone.Data;
using PinterestClone.Models;
using System.Threading.Tasks;
using System.Linq;

namespace PinterestClone.Controllers
{
    public class BoardController : Controller
    {
    private readonly ApplicationDbContext _context;

    public BoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Board
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var boards = await _context.Boards
                .Where(b => b.UserId == userId.Value)
                .Include(b => b.PinBoards)
                    .ThenInclude(pb => pb.Pin)
                .ToListAsync();
            return View(boards);
        }

        // GET: /Board/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            return View();
        }

        // POST: /Board/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Board board)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                board.UserId = userId.Value;
                _context.Boards.Add(board);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(board);
        }
    }
}
