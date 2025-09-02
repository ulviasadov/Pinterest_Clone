using Microsoft.AspNetCore.Mvc;
using PinterestClone.Data;
using PinterestClone.Models;
using System.Security.Cryptography;
using System.Text;

namespace PinterestClone.Controllers
{
    public class UserController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBio(string Bio)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session not found. Please log in again.";
                return RedirectToAction("Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Profile");
            }
            user.Bio = Bio;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Bio updated successfully.";
            return RedirectToAction("Profile");
        }

        // GET: /User/Dashboard
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null || !user.IsAdmin)
                return Unauthorized();
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.PinCount = _context.Pins.Count();
            ViewBag.BoardCount = _context.Boards.Count();
            ViewBag.ReportCount = _context.Set<PinReport>().Count();
            ViewBag.TopUsers = _context.Users
                .OrderByDescending(u => _context.Pins.Count(p => p.UserId == u.Id) + _context.Boards.Count(b => b.UserId == u.Id))
                .Take(5)
                .ToList();
            ViewBag.TopPins = _context.Pins
                .OrderByDescending(p => _context.Set<PinReport>().Count(r => r.PinId == p.Id))
                .Take(5)
                .ToList();
            ViewBag.TopBoards = _context.Boards
                .OrderByDescending(b => b.PinBoards.Count)
                .Take(5)
                .ToList();
            return View();
        }

        // GET: /User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // POST: /User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            RestoreSessionFromCookies();
            // Remove PasswordHash from ModelState (empty during form binding)
            ModelState.Remove("PasswordHash");

            // Debug: Check ModelState errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                return View(user);
            }

            // Email check
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(user);
            }

            try
            {
                // Hash the password
                var passwordHash = HashPassword(user.Password);
                // Clear the Password property (do not save to database)
                user.Password = string.Empty;
                // Prepare user data for token
                var userData = $"{user.Name}|{user.Email}|{passwordHash}";
                var token = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(userData));
                var confirmationUrl = Url.Action("ConfirmEmail", "User", new { token }, Request.Scheme);
                var emailService = new PinterestClone.Services.EmailService();
                var subject = "PinterestClone - Email Confirmation";
                var body = $"<p>Your registration was successful. To confirm your account, <a href='{confirmationUrl}'>click here</a>.</p>";
                emailService.SendEmail(user.Email, subject, body);

                TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred during registration: {ex.Message}");
                return View(user);
            }

        }

        public IActionResult ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest();
            var decoded = System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(token));
            var parts = decoded.Split('|');
            if (parts.Length != 3) return BadRequest();
            var name = parts[0];
            var email = parts[1];
            var passwordHash = parts[2];
            // Check if user already exists
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    TempData["SuccessMessage"] = "Email already confirmed.";
                    return RedirectToAction("Login");
                }
                user.EmailConfirmed = true;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Email confirmed successfully. You can now log in.";
                return RedirectToAction("Login");
            }
            // Add new user to database
            var newUser = new User
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                EmailConfirmed = true
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Email confirmed successfully. You can now log in.";
            return RedirectToAction("Login");
        }

        // GET: /User/Login
        public IActionResult Login()
        {
            RestoreSessionFromCookies();
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            RestoreSessionFromCookies();
            // Check for empty fields
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password fields cannot be empty.");
                return View();
            }

            var hash = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email address not confirmed. Please check your email.");
                    return View();
                }
                // Login successful, set session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "");

                if (rememberMe)
                {
                    // Persistent cookie (example, for UserId)
                    Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(14),
                        IsEssential = true
                    });
                    Response.Cookies.Append("UserName", user.Name, new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(14),
                        IsEssential = true
                    });
                    Response.Cookies.Append("ProfileImagePath", user.ProfileImagePath ?? "", new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(14),
                        IsEssential = true
                    });
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        // GET: /User/Logout
        public IActionResult Logout()
        {
            RestoreSessionFromCookies();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /User/Profile/{id?}
        public IActionResult Profile(int? id)
        {
            RestoreSessionFromCookies();
            int userId;
            if (id.HasValue)
            {
                userId = id.Value;
            }
            else
            {
                var sessionUserId = HttpContext.Session.GetInt32("UserId");
                if (sessionUserId == null)
                {
                    // Try to restore session from cookies
                    var cookieUserId = Request.Cookies["UserId"];
                    var cookieUserName = Request.Cookies["UserName"];
                    if (!string.IsNullOrEmpty(cookieUserId) && int.TryParse(cookieUserId, out int parsedUserId))
                    {
                        HttpContext.Session.SetInt32("UserId", parsedUserId);
                        if (!string.IsNullOrEmpty(cookieUserName))
                            HttpContext.Session.SetString("UserName", cookieUserName);
                        userId = parsedUserId;
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    userId = sessionUserId.Value;
                }
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();
            var pins = _context.Pins.Where(p => p.UserId == userId).OrderByDescending(p => p.Id).ToList();
            var boards = _context.Boards.Where(b => b.UserId == userId && (!b.IsPrivate || userId == HttpContext.Session.GetInt32("UserId"))).OrderByDescending(b => b.Id).ToList();
            var followersList = _context.Follows.Where(f => f.FollowingId == userId).Select(f => f.Follower).ToList();
            var followingList = _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.Following).ToList();
            ViewBag.FollowersList = followersList;
            ViewBag.FollowingList = followingList;
            var followersCount = _context.Follows.Count(f => f.FollowingId == userId);
            var followingCount = _context.Follows.Count(f => f.FollowerId == userId);
            ViewBag.Pins = pins;
            ViewBag.Boards = boards;
            ViewBag.PinCount = pins.Count;
            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
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
            ViewBag.ReportedPins = _context.Set<PinReport>().ToList();
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
                ModelState.AddModelError("Email", "This email address is already in use.");
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
                ModelState.AddModelError("Email", "This email address is already in use.");
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
        [HttpGet]
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadProfilePhoto(IFormFile profileImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session not found. Please log in again.";
                return RedirectToAction("Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Profile");
            }
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a photo.";
                return RedirectToAction("Profile");
            }
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = $"user_{user.Id}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }
                user.ProfileImagePath = $"/uploads/{fileName}";
                _context.SaveChanges();
                HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "");
                Response.Cookies.Append("ProfileImagePath", user.ProfileImagePath ?? "", new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(14),
                    IsEssential = true
                });
                TempData["SuccessMessage"] = "Profile photo uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading photo: {ex.Message}";
            }
            return RedirectToAction("Profile");
        }
    }
}