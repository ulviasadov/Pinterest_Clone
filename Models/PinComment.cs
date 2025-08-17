using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinterestClone.Models
{
    public class PinComment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int PinId { get; set; }
        public Pin Pin { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
