using System;
using System.Collections.Generic;

namespace SupportTicketSystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; } // Unique ID for each ticket
        public required string Title { get; set; } // Title of the ticket
        public required string Description { get; set; } // Description of the issue
        public DateTime CreatedAt { get; set; } // Date and time when the ticket was created
        public DateTime? UpdatedAt { get; set; } // Date and time when the ticket was last updated (nullable)
        public string Status { get; set; } = "Open"; // Status of the ticket (Open, In Progress, Closed)
        public required string Priority { get; set; } // Priority level (e.g., Low, Medium, High)
        
        // Collection of replies to store conversation history
        public List<TicketReply> Replies { get; set; } = new List<TicketReply>();
        
        // Foreign key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } // Navigation property to Category
        
        // Foreign key for User (the user who created the ticket)
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property to User (Customer or Creator)
        
        // Foreign key for the Agent assigned to the ticket
        public int? AgentId { get; set; } // Nullable because the ticket may not yet have an agent assigned
        public User? Agent { get; set; } // Navigation property to User (Agent) - Nullable because an agent may not be assigned initially
    }

    public class TicketReply
    {
        public int TicketReplyId { get; set; } // Unique ID for each reply
        public int TicketId { get; set; } // Foreign key for Ticket
        public Ticket Ticket { get; set; } // Navigation property to Ticket
        public required string Message { get; set; } // Content of the reply (now required)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of the reply
        public int UserId { get; set; } // ID of the user or agent replying
        public User User { get; set; } // Navigation property to User (Agent or Customer replying)
    }
}
