using System.ComponentModel.DataAnnotations;

namespace PinterestClone.Models
{
    public class Pin
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = "Genel";

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }

        public List<PinBoard> PinBoards { get; set; } = new();
        public List<PinLike> PinLikes { get; set; } = new();
        public List<PinComment> PinComments { get; set; } = new();
    }
}