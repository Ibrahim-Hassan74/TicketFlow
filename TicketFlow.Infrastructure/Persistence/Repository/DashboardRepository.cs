using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Application.Common.Models;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Infrastructure.Persistence.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsResponse> GetDashboardStatsAsync(CancellationToken ct = default)
        {
            var stats = new DashboardStatsResponse
            {
                TotalTickets = await _context.Tickets.CountAsync(ct),
                OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open, ct),
                InProgressTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress, ct),
                ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Resolved, ct),
                ClosedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed, ct),
                CriticalOpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open && t.Priority == TicketPriority.Critical, ct)
            };

            var resolvedTickets = await _context.Tickets
                .AsNoTracking()
                .Where(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.ResolvedAt != null)
                .Select(t => new { t.CreatedAt, t.ResolvedAt })
                .ToListAsync(ct);

            if (resolvedTickets.Count != 0)
            {
                var totalHours = resolvedTickets.Sum(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
                stats.AvgResolutionTimeHours = Math.Round(totalHours / resolvedTickets.Count, 2);
            }

            return stats;
        }

        public async Task<IEnumerable<AgentWorkloadResponse>> GetAgentWorkloadAsync(CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.AssignedTickets.Any())
                .Select(u => new AgentWorkloadResponse
                {
                    AgentId = u.Id,
                    AgentName = u.DisplayName,
                    OpenTickets = u.AssignedTickets.Count(t => t.Status == TicketStatus.Open),
                    InProgressTickets = u.AssignedTickets.Count(t => t.Status == TicketStatus.InProgress),
                    TotalTimeMinutes = u.TimeEntries.Sum(te => te.DurationMinutes)
                })
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TicketTrendResponse>> GetTicketTrendsAsync(int days, CancellationToken ct = default)
        {
            var cutoff = DateTimeOffset.UtcNow.AddDays(-days);

            var tickets = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.CreatedAt >= cutoff || (t.ClosedAt != null && t.ClosedAt >= cutoff))
                .Select(t => new { t.CreatedAt, t.ClosedAt })
                .ToListAsync(ct);

            var trends = new List<TicketTrendResponse>();

            for (int i = 0; i < days; i++)
            {
                var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i));
                trends.Add(new TicketTrendResponse
                {
                    Date = date,
                    OpenCount = tickets.Count(t => DateOnly.FromDateTime(t.CreatedAt.Date) == date),
                    ClosedCount = tickets.Count(t => t.ClosedAt != null && DateOnly.FromDateTime(t.ClosedAt.Value.Date) == date)
                });
            }

            return trends.OrderBy(t => t.Date).ToList();
        }
    }
}
