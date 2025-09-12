using System.ComponentModel.DataAnnotations;

namespace PinterestClone.Models
{
    public class PinReport
    {
        public int Id { get; set; }
        public int PinId { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
        public Pin? Pin { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.Now;
    }
}
