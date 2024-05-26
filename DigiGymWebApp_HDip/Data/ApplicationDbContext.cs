using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Food> FoodDiary { get; set; }
        public DbSet<Water> WaterEntries { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Goal>()
                .HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Workout>()
                .HasOne(w => w.User)
                .WithMany(u => u.Workouts)
                .HasForeignKey(w => w.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Food>()
                .HasOne(f => f.User)
                .WithMany(u => u.FoodDiary)
                .HasForeignKey(f => f.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Water>()
                .HasOne(w => w.User)
                .WithMany(u => u.WaterEntries)
                .HasForeignKey(w => w.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}