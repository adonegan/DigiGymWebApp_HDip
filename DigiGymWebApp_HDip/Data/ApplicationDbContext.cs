using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Food> FoodDiary { get; set; }
        public DbSet<Water> WaterEntries { get; set; }
        public DbSet<UserProfile> ProfileEntries { get; set; }
        public DbSet<WeightEntry> WeightEntries { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithMany(u => u.ProfileEntries)
                .HasForeignKey(w => w.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeightEntry>()
                .HasOne(we => we.User)
                .WithMany(w => w.WeightEntries)
                .HasForeignKey(f => f.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationID);

            builder.Entity<Conversation>()
                .HasOne(c => c.Client)
                .WithMany(u => u.ClientConversations)
                .HasForeignKey(c => c.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(t => t.Trainer)
                .WithMany(u => u.TrainerConversations)
                .HasForeignKey(t => t.TrainerID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(s => s.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(s => s.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(r => r.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(u => u.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}