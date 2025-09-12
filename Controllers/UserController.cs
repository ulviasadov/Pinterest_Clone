using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterestClone.Data;
using PinterestClone.Models;
using PinterestClone.Services;
using System.Security.Cryptography;
using System.Text;

namespace PinterestClone.Controllers
{
    public class UserController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public UserController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        #region Registration & Email Confirmation

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid) return View(user);

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(user);
            }

            var passwordHash = HashPassword(user.Password);
            user.Password = string.Empty; // clear plain password
            user.PasswordHash = passwordHash;

            // generate email confirmation token
            var tokenBytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            user.EmailConfirmationToken = Convert.ToBase64String(tokenBytes);

            _context.Users.Add(user);
            _context.SaveChanges();

            var confirmationUrl = Url.Action("ConfirmEmail", "User", new { token = user.EmailConfirmationToken }, Request.Scheme);
            var subject = "PinterestClone - Email Confirmation";
            var body = $"<p>Welcome {user.Name}! Please confirm your email by <a href='{confirmationUrl}'>clicking here</a>.</p>";

            _emailService.SendEmail(user.Email, subject, body);

            TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return View("Error", "Home");

            var user = _context.Users.FirstOrDefault(u => u.EmailConfirmationToken == token);
            if (user == null) return View("Error", "Home");

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Email confirmed successfully. You can now log in.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Login / Logout

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password fields cannot be empty.");
                return View();
            }

            var hash = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Email not confirmed. Please check your email.");
                return View();
            }

            // set session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Profile & Bio

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
                return View("Error", "Home");
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

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            bool isFollowing = false;

            if (currentUserId.HasValue)
            {
                isFollowing = _context.Follows.Any(f => f.FollowerId == currentUserId.Value && f.FollowingId == userId);
            }

            ViewBag.IsFollowing = isFollowing;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBio(string bio)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null) return View("Error", "Home");

            user.Bio = bio;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Bio updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadProfilePhoto(IFormFile profileImage)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null) return View("Error", "Home");

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"user_{user.Id}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                user.ProfileImagePath = $"/uploads/{fileName}";
                _context.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveProfilePhoto()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null) return View("Error", "Home");

            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                user.ProfileImagePath = null;
                _context.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult EditUsername(string newUsername)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return View("Error", "Home");

            user.Name = newUsername;

            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        #endregion

        #region Forgot / Reset Password

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Email not found.";
                return RedirectToAction("ForgotPassword");
            }

            // generate token
            user.ResetPasswordToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            _context.SaveChanges();

            var resetUrl = Url.Action("ResetPassword", "User", new { token = user.ResetPasswordToken }, Request.Scheme);
            var subject = "PinterestClone - Reset Password";
            var body = $"<p>To reset your password, <a href='{resetUrl}'>click here</a>. This link expires in 1 hour.</p>";
            _emailService.SendEmail(user.Email, subject, body);

            TempData["SuccessMessage"] = "Password reset link sent to your email.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest();
            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);
            if (user == null) return BadRequest();

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid or expired token.";
                return RedirectToAction("ForgotPassword");
            }

            user.PasswordHash = HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password reset successfully. You can now log in.";
            return RedirectToAction("Login");
        }

        #endregion

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // GET: /User/AdminPanel
        public IActionResult AdminPanel(string? userSearch)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null || !user.IsAdmin)
                return View("Error", "Home");
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
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");
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
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");
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
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return View("Error", "Home");
            return View(user);
        }

        // POST: /User/AdminEditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEditUser(int id, string name, string email, bool isAdmin)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var admin = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return View("Error", "Home");

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

            var admin = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.IsAdmin) return View("Error", "Home");

            var follows = _context.Follows.Where(f => f.FollowerId == id || f.FollowingId == id).ToList();

            _context.Follows.RemoveRange(follows);

            var boards = _context.Boards.Where(b => b.UserId == id).ToList();
            foreach(var b in boards)
            {
                var pinBoards = _context.PinBoards.Where(pb => pb.BoardId == b.Id).ToList();
                _context.PinBoards.RemoveRange(pinBoards);
            };

            _context.Boards.RemoveRange(boards);

            var pins = _context.Pins.Where(p => p.UserId == id).ToList();
            foreach(var pin in pins)
            {
                var pinBoards = _context.PinBoards.Where(pb => pb.PinId == pin.Id).ToList();
                _context.PinBoards.RemoveRange(pinBoards);
            }

            _context.Pins.RemoveRange(pins);

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult DeletePin(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var admin = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");

            var pin = _context.Pins.Include(p => p.PinComments).FirstOrDefault(p => p.Id == id);
            if (pin == null) return View("Error", "Home");

            _context.PinComments.RemoveRange(pin.PinComments);
            _context.Pins.Remove(pin);
            _context.SaveChanges();

            return RedirectToAction("AdminPanel");
        }

        [HttpGet]
        public IActionResult DeleteBoard(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var admin = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (admin == null || !admin.IsAdmin) return View("Error", "Home");

            var board = _context.Boards.Include(b => b.PinBoards).FirstOrDefault(b => b.Id == id);
            if (board == null) return View("Error", "Home");

            _context.PinBoards.RemoveRange(board.PinBoards);
            _context.Boards.Remove(board);
            _context.SaveChanges();

            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public IActionResult EditBoard(Board board)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var boardUserId = _context.Boards.FirstOrDefault(b => b.UserId == userId);
            if (boardUserId == null) return View("Error", "Home");

            

            return RedirectToAction("Profile");
        }

        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null || !user.IsAdmin)
                return View("Error", "Home");

            ViewBag.UserCount = _context.Users.Count();
            ViewBag.PinCount = _context.Pins.Count();
            ViewBag.BoardCount = _context.Boards.Count();
            ViewBag.ReportCount = _context.PinReports.Count();
            ViewBag.TopUsers = _context.Users.OrderByDescending(u => u.Pins.Count).Take(5).ToList();
            ViewBag.TopPins = _context.Pins.OrderByDescending(p => p.PinReports.Count()).Take(5).ToList();
            ViewBag.TopBoards = _context.Boards.OrderByDescending(b => b.PinBoards.Count()).Take(5).ToList();
            ViewBag.IsAdmin = user.IsAdmin;

            return View();
        }
    }
}
