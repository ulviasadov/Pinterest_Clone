using System.Collections.Generic;

namespace PinterestClone.ViewModels
{
    public class BoardListViewModel
    {
        public IEnumerable<PinterestClone.Models.Board> Boards { get; set; } = new List<PinterestClone.Models.Board>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
