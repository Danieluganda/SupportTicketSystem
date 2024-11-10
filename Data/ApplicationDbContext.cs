using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Models; // Import the Models namespace

namespace SupportTicketSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets represent the tables in the database
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; } // Ensure this matches the model name (TicketReply)

        // Override the OnModelCreating method to configure relationships and table mappings
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TicketReply relationships
            modelBuilder.Entity<TicketReply>()
                .HasOne(tr => tr.Ticket)  // A TicketReply belongs to a Ticket
                .WithMany(t => t.Replies) // A Ticket can have many replies
                .HasForeignKey(tr => tr.TicketId)
                .OnDelete(DeleteBehavior.Cascade);  // Deleting a Ticket deletes related replies

            modelBuilder.Entity<TicketReply>()
                .HasOne(tr => tr.User)  // A TicketReply is created by a User
                .WithMany(u => u.Replies) // A User can create many replies
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Deleting a User deletes related replies

            // Configure User and Ticket relationship (one-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tickets) // A User can create many Tickets
                .WithOne(t => t.User) // A Ticket is created by one User
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);  // Deleting a User does not delete their tickets (Set null for UserId)

            // Configure Category and Ticket relationship (one-to-many)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Tickets) // A Category can have many Tickets
                .WithOne(t => t.Category) // A Ticket belongs to one Category
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);  // Deleting a Category does not delete their tickets (Set null for CategoryId)

            // Additional configurations for other entities can go here
        }
    }
}
