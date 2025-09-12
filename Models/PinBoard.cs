namespace PinterestClone.Models
{
    public class PinBoard
    {
        public int PinId { get; set; }
        public Pin Pin { get; set; } = null!;

        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;
    }
}
