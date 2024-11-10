using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SupportTicketSystem.Data;
using SupportTicketSystem.Models;

namespace SupportTicketSystem.Services
{
    public class TicketAssignmentService : IHostedService, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketAssignmentService> _logger;
        private Timer _timer;

        public TicketAssignmentService(ApplicationDbContext context, ILogger<TicketAssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run the ticket assignment every 5 minutes
            _timer = new Timer(AssignTickets, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));  
            return Task.CompletedTask;
        }

        private async void AssignTickets(object state)
        {
            try
            {
                // Retrieve all unassigned tickets
                var unassignedTickets = await _context.Tickets
                    .Where(t => t.AgentId == null)  // Find tickets with no agent assigned
                    .ToListAsync();

                foreach (var ticket in unassignedTickets)
                {
                    // Example logic: Assign to the first available agent
                    var availableAgent = await _context.Users
                        .Where(u => u.Role == Role.Agent && !u.Tickets.Any()) // Check for an agent with no tickets
                        .FirstOrDefaultAsync();

                    if (availableAgent != null)
                    {
                        ticket.AgentId = availableAgent.UserId;
                        await _context.SaveChangesAsync();  // Save the assignment change to the database immediately
                        _logger.LogInformation($"Ticket {ticket.TicketId} auto-assigned to Agent {availableAgent.UserId}");
                    }
                    else
                    {
                        _logger.LogWarning($"No available agent for Ticket {ticket.TicketId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during ticket assignment: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
