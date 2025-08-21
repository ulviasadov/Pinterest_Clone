using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PinterestClone.Models
{
    public class Board
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

    public List<PinBoard> PinBoards { get; set; } = new();

    public string? CoverImagePath { get; set; }
    }
}
