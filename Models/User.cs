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

        // Form binding için geçici property (veritabanına kaydedilmez)
        [NotMapped]
        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [MinLength(3, ErrorMessage = "Şifre en az 3 karakter olmalıdır")]
        public string Password { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;

    public string? ProfileImagePath { get; set; }

    public List<PinLike> PinLikes { get; set; } = new();
    public List<PinComment> PinComments { get; set; } = new();
    }
} 