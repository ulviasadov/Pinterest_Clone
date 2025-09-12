using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public string ImagePath { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }

        public List<PinBoard> PinBoards { get; set; } = new();
        public List<PinLike> PinLikes { get; set; } = new();
        public List<PinComment> PinComments { get; set; } = new();
        public List<PinReport> PinReports { get; set; } = new();
    }
}