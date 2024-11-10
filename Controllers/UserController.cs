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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Users/register
        // Register a new user with a default role (e.g., Customer)
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(User user)
        {
            // Assign default role (Customer) if not provided
            if (user.Role == 0) // Role should not be null, 0 indicates default role (Customer)
            {
                user.Role = Role.Customer;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user); // Return 201 with the new user
        }

        // GET: api/Users
        // Get all users (admins can access this endpoint)
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only admins can view all users
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        // Get a specific user by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Agent,Customer")] // Allow admins, agents, and customers to view their own info
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if user is not found
            }

            // Admins and agents can view other users, customers can only view themselves
            if (User.IsInRole("Admin") || User.IsInRole("Agent") || User.Identity.Name == user.Username)
            {
                return Ok(user); // Return the found user
            }

            return Unauthorized(); // Return 401 if unauthorized to view this user
        }

        // PUT: api/Users/5/role
        // Update the role of an existing user (only admins can do this)
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")] // Only admins can update roles
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] Role role)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if the user doesn't exist
            }

            user.Role = role;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content to indicate success
        }

        // DELETE: api/Users/5
        // Delete a user (only admins can delete users)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete users
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(); // Return 404 if user is not found
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content to indicate success
        }

        // Helper method to check if the user exists
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
