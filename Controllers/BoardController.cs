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

            int page = 1;
            int pageSize = 6;
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out page);
                if (page < 1) page = 1;
            }

            var query = _context.Boards
                .Where(b => b.UserId == userId.Value)
                .Include(b => b.PinBoards)
                    .ThenInclude(pb => pb.Pin);

            int totalBoards = await query.CountAsync();
            int totalPages = (int)System.Math.Ceiling(totalBoards / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var boards = await query
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PinterestClone.ViewModels.BoardListViewModel
            {
                Boards = boards,
                CurrentPage = page,
                TotalPages = totalPages
            };
            return View(model);
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
