using Microsoft.EntityFrameworkCore;
using PinterestClone.Models;

namespace PinterestClone.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Pin> Pins { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<PinBoard> PinBoards { get; set; }
        public DbSet<PinLike> PinLikes { get; set; }
        public DbSet<PinComment> PinComments { get; set; }
        public DbSet<Follow> Follows { get; set; }
    public DbSet<PinReport> PinReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PinBoard>()
                .HasKey(pb => new { pb.PinId, pb.BoardId });
            modelBuilder.Entity<PinBoard>()
                .HasOne(pb => pb.Pin)
                .WithMany(p => p.PinBoards)
                .HasForeignKey(pb => pb.PinId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PinBoard>()
                .HasOne(pb => pb.Board)
                .WithMany(b => b.PinBoards)
                .HasForeignKey(pb => pb.BoardId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PinLike>()
                .HasKey(pl => new { pl.PinId, pl.UserId });
            modelBuilder.Entity<PinLike>()
                .HasOne(pl => pl.Pin)
                .WithMany(p => p.PinLikes)
                .HasForeignKey(pl => pl.PinId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PinLike>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.PinLikes)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PinComment>()
                .HasOne(pc => pc.Pin)
                .WithMany(p => p.PinComments)
                .HasForeignKey(pc => pc.PinId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PinComment>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.PinComments)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
