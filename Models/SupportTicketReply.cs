using System;

namespace SupportTicketSystem.Models
{
    public class SupportTicketReply  // Renamed class
    {
        public int ReplyId { get; set; } // Primary Key

        public int TicketId { get; set; } // Foreign Key to Ticket
        public Ticket Ticket { get; set; } // Navigation property to Ticket

        public int UserId { get; set; } // Foreign Key to User (who made the reply)
        public User User { get; set; } // Navigation property to User

        public required string Content { get; set; } // Content of the reply (now required)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // When the reply was created
        public DateTime? UpdatedAt { get; set; } // When the reply was last updated (nullable)
    }
}
