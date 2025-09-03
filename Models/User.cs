using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinterestClone.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Password field is required")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters")]
        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;

        public string? ProfileImagePath { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }

        public string? Bio { get; set; }

        public List<PinLike> PinLikes { get; set; } = new();
        public List<PinComment> PinComments { get; set; } = new();
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

    }
}