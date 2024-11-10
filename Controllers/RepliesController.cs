using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Data;
using SupportTicketSystem.Models;
using SupportTicketSystem.Services;  // Add this for AIReplyService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportTicketSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepliesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AIReplyService _aiReplyService; // Inject AIReplyService

        public RepliesController(ApplicationDbContext context, AIReplyService aiReplyService)
        {
            _context = context;
            _aiReplyService = aiReplyService; // Assign the AI service to a local variable
        }

        // GET: api/Replies/ticket/{ticketId}
        [HttpGet("ticket/{ticketId}")]
        [Authorize] // Requires the user to be authenticated
        public async Task<ActionResult<IEnumerable<TicketReply>>> GetRepliesByTicket(int ticketId)
        {
            var replies = await _context.TicketReplies
                .Include(r => r.User) 
                .Where(r => r.TicketId == ticketId)
                .ToListAsync();

            if (replies == null || !replies.Any())
                return NotFound();

            return Ok(replies);
        }

        // GET: api/Replies/{id}
        [HttpGet("{id}")]
        [Authorize] // Requires the user to be authenticated
        public async Task<ActionResult<TicketReply>> GetReply(int id)
        {
            var reply = await _context.TicketReplies
                .Include(r => r.User) 
                .FirstOrDefaultAsync(r => r.TicketReplyId == id);

            if (reply == null)
                return NotFound();

            return Ok(reply);
        }

        // POST: api/Replies
        [HttpPost]
        [Authorize] // Requires the user to be authenticated
        public async Task<ActionResult<TicketReply>> CreateReply(TicketReply reply)
        {
            // Ensure the related ticket exists before allowing a reply
            var ticketExists = await _context.Tickets.AnyAsync(t => t.TicketId == reply.TicketId);
            if (!ticketExists)
                return BadRequest("Ticket not found.");

            // Set the creation time of the reply
            reply.CreatedAt = System.DateTime.UtcNow;

            // Get the current authenticated user (OAuth or JWT)
            var currentUserEmail = User.Identity.Name; // Assuming the email claim is used for authentication
            reply.User = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            // Generate an AI-powered reply suggestion based on the ticket content
            var aiReplySuggestion = await _aiReplyService.GetSuggestedReply(reply.TicketId.ToString());
            if (!string.IsNullOrEmpty(aiReplySuggestion))
            {
                // Optionally, you can set the AI reply suggestion as the message of the new reply
                reply.Message = aiReplySuggestion;
            }

            // Add the reply to the database
            _context.TicketReplies.Add(reply);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReply), new { id = reply.TicketReplyId }, reply);
        }

        // PUT: api/Replies/{id}
        [HttpPut("{id}")]
        [Authorize] // Requires the user to be authenticated
        public async Task<IActionResult> UpdateReply(int id, TicketReply updatedReply)
        {
            if (id != updatedReply.TicketReplyId)
                return BadRequest("Reply ID mismatch.");

            var reply = await _context.TicketReplies.FindAsync(id);
            if (reply == null)
                return NotFound();

            // Only allow the user who created the reply to update it
            if (reply.User.Email != User.Identity.Name)
                return Unauthorized();

            reply.Message = updatedReply.Message;
            reply.CreatedAt = updatedReply.CreatedAt;

            _context.Entry(reply).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Replies/{id}
        [HttpDelete("{id}")]
        [Authorize] // Requires the user to be authenticated
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _context.TicketReplies.FindAsync(id);
            if (reply == null)
                return NotFound();

            if (reply.User.Email != User.Identity.Name)
                return Unauthorized();

            _context.TicketReplies.Remove(reply);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReplyExists(int id)
        {
            return _context.TicketReplies.Any(e => e.TicketReplyId == id);
        }
    }
}
