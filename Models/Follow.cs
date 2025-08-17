using System;

namespace PinterestClone.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; } // Takip eden kullanıcı
        public int FollowingId { get; set; } // Takip edilen kullanıcı
        public DateTime FollowedAt { get; set; }
    }
}