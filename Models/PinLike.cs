using System.ComponentModel.DataAnnotations.Schema;

namespace PinterestClone.Models
{
    public class PinLike
    {
        public int PinId { get; set; }
        public Pin Pin { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
