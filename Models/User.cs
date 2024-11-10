using System.Collections.Generic;

namespace SupportTicketSystem.Models
{
    // Enum for defining roles
    public enum Role
    {
        Admin,
        Agent,
        Customer
    }

    public class User
    {
        public int UserId { get; set; } // Unique ID for each user
        public string Username { get; set; } // Username of the user
        public string Email { get; set; } // Email of the user
        public Role Role { get; set; } // Role of the user (Admin, Agent, Customer)

        // OAuth-related fields
        public string OAuthProvider { get; set; } // The provider (Google, Facebook, etc.)
        public string OAuthProviderUserId { get; set; } // The unique user ID from the OAuth provider
        public string AccessToken { get; set; } // (Optional) Store access token for OAuth

        // Navigation property to list of tickets created by this user
        public List<Ticket> Tickets { get; set; } = new List<Ticket>(); // Initialize the Tickets list to avoid null reference

        // Navigation property to list of replies created by this user
        public List<TicketReply> Replies { get; set; } = new List<TicketReply>(); // Initialize the Replies list to avoid null reference
    }
}
