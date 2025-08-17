using Microsoft.AspNetCore.Mvc;
using PinterestClone.Data;
using PinterestClone.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace PinterestClone.Controllers
{
    public class UserController : Controller
    {
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            // PasswordHash alanını ModelState'den çıkar (form binding sırasında boş geliyor)
            ModelState.Remove("PasswordHash");

            // Debug: ModelState hatalarını kontrol et
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View(user);
            }

            // Email kontrolü
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
                return View(user);
            }

            try
            {
                // Şifreyi hashle
                user.PasswordHash = HashPassword(user.Password);
                
                // Password property'sini temizle (veritabanına kaydedilmesin)
                user.Password = string.Empty;

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kayıt sırasında hata oluştu: {ex.Message}");
                return View(user);
            }
        }

        // GET: /User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email ve şifre alanları boş olamaz.");
                return View();
            }

            var hash = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);

            if (user != null)
            {
                // Giriş başarılı, session'a yaz
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Geçersiz email veya şifre");
            return View();
        }

        // GET: /User/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /User/Profile/{id?}
        public IActionResult Profile(int? id)
        {
            int userId;
            if (id.HasValue)
            {
                userId = id.Value;
            }
            else
            {
                var sessionUserId = HttpContext.Session.GetInt32("UserId");
                if (sessionUserId == null)
                    return RedirectToAction("Login");
                userId = sessionUserId.Value;
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();
            var pins = _context.Pins.Where(p => p.UserId == userId).OrderByDescending(p => p.Id).ToList();
            var boards = _context.Boards.Where(b => b.UserId == userId).OrderByDescending(b => b.Id).ToList();
            ViewBag.Pins = pins;
            ViewBag.Boards = boards;
            return View(user);
        }

        // GET: /User/AdminPanel
        public IActionResult AdminPanel(string? userSearch)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null || !user.IsAdmin)
                return Unauthorized();
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(userSearch))
            {
                users = users.Where(u => u.Name.Contains(userSearch) || u.Email.Contains(userSearch));
            }
            ViewBag.Users = users.ToList();
            ViewBag.Pins = _context.Pins.ToList();
            ViewBag.Boards = _context.Boards.ToList();
            return View();
        }

        // GET: /User/AdminAddUser
        public IActionResult AdminAddUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            return View();
        }

        // POST: /User/AdminAddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminAddUser(User user, bool isAdmin)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
                return View(user);
            }
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            user.PasswordHash = HashPassword(user.Password);
            user.Password = string.Empty;
            user.IsAdmin = isAdmin;
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("AdminPanel");
        }

        // GET: /User/AdminEditUser/5
        public IActionResult AdminEditUser(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /User/AdminEditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEditUser(int id, string name, string email, bool isAdmin)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            if (_context.Users.Any(u => u.Email == email && u.Id != id))
            {
                ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
                return View(user);
            }
            user.Name = name;
            user.Email = email;
            user.IsAdmin = isAdmin;
            _context.SaveChanges();
            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.IsAdmin) return NotFound();
            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult DeletePin(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            var pin = _context.Pins.FirstOrDefault(p => p.Id == id);
            if (pin == null) return NotFound();
            _context.Pins.Remove(pin);
            _context.SaveChanges();
            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult DeleteBoard(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var admin = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (admin == null || !admin.IsAdmin) return Unauthorized();
            var board = _context.Boards.FirstOrDefault(b => b.Id == id);
            if (board == null) return NotFound();
            _context.Boards.Remove(board);
            _context.SaveChanges();
            return RedirectToAction("AdminPanel");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
} 