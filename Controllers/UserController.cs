using Microsoft.AspNetCore.Mvc;
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
            if (string.IsNullOrEmpty(token)) return BadRequest();

            var user = _context.Users.FirstOrDefault(u => u.EmailConfirmationToken == token);
            if (user == null) return BadRequest();

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

        [HttpGet]
        public IActionResult Profile(int? id)
        {
            int userId = id ?? HttpContext.Session.GetInt32("UserId") ?? -1;
            if (userId == -1) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBio(string bio)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null) return NotFound();

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
            if (user == null) return NotFound();

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
    }
}
