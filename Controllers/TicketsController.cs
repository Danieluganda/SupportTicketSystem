using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Data;
using SupportTicketSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportTicketSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Only users with the 'Admin' or 'Agent' role can access this endpoint
        [HttpGet]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Category)  // Include the associated Category
                .Include(t => t.User)      // Include the associated User (creator of the ticket)
                .Include(t => t.Replies)   // Include the replies to the ticket
                .ToListAsync();

            return Ok(tickets);
        }

        // Only users with the 'Admin' role can access this endpoint
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Category)  // Include the associated Category
                .Include(t => t.User)      // Include the associated User
                .Include(t => t.Replies)   // Include the replies to the ticket
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound(); // Return 404 if ticket is not found
            }

            return Ok(ticket); // Return the found ticket
        }

        // Only users with the 'Customer' role can create a ticket
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<Ticket>> CreateTicket(Ticket ticket)
        {
            // Ensure the related Category and User are valid
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == ticket.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Category not found.");
            }

            // Set the creation timestamp (optional)
            ticket.CreatedAt = System.DateTime.UtcNow;

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.TicketId }, ticket); // Return 201 with the created ticket
        }

        // Only users with the 'Admin' or 'Agent' role can update a ticket
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> UpdateTicket(int id, Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return BadRequest("Ticket ID mismatch."); // Ensure the IDs match
            }

            // Check if the ticket exists
            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
            {
                return NotFound(); // Return 404 if the ticket doesn't exist
            }

            // Update fields as necessary
            existingTicket.Title = ticket.Title;
            existingTicket.Description = ticket.Description;
            existingTicket.Status = ticket.Status;
            existingTicket.Priority = ticket.Priority;
            existingTicket.CategoryId = ticket.CategoryId;

            // Update the timestamp (optional)
            existingTicket.UpdatedAt = System.DateTime.UtcNow;

            _context.Entry(existingTicket).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content to indicate success
        }

        // Only users with the 'Admin' role can delete a ticket
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(); // Return 404 if the ticket doesn't exist
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content to indicate success
        }

        // Helper method to check if a ticket exists
        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }
    }
}
