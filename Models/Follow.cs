using System;

namespace PinterestClone.Models
{
    public class Follow
    {
        public int Id { get; set; }
    public int FollowerId { get; set; } // Takip eden kullanıcı
    public User Follower { get; set; } // Navigation property
    public int FollowingId { get; set; } // Takip edilen kullanıcı
    public User Following { get; set; } // Navigation property
    public DateTime FollowedAt { get; set; }
    }
}