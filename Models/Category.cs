using System.Collections.Generic;

namespace SupportTicketSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; } // Unique ID for each category
        public string Name { get; set; } // Name of the category (e.g., Billing, Technical)

        // Navigation property to list of tickets in this category
        public List<Ticket> Tickets { get; set; } = new List<Ticket>(); // Initialize to prevent null reference
    }
}
