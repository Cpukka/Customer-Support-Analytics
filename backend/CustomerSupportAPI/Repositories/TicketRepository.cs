using Microsoft.EntityFrameworkCore;
using CustomerSupportAPI.Data;
using CustomerSupportAPI.Models;
using Microsoft.Extensions.Logging;

namespace CustomerSupportAPI.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketRepository> _logger;

        public TicketRepository(ApplicationDbContext context, ILogger<TicketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ticket {ticket.TicketID} created successfully");
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.AIAnalysis)
                .FirstOrDefaultAsync(t => t.TicketID == id);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int page, int pageSize)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.AIAnalysis)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalTicketCountAsync()
        {
            return await _context.Tickets.CountAsync();
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Entry(ticket).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<AnalyticsSummary> GetAnalyticsSummaryAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var tickets = await _context.Tickets
                .Where(t => t.CreatedAt >= thirtyDaysAgo)
                .Include(t => t.AIAnalysis)
                .ToListAsync();

            var closedTickets = tickets.Where(t => t.Status == "Closed");
            var avgResolutionTime = closedTickets.Any() 
                ? closedTickets.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours) 
                : 0;

            var sentimentDistribution = tickets
                .Where(t => t.AIAnalysis != null)
                .GroupBy(t => t.AIAnalysis!.SentimentLabel)
                .ToDictionary(g => g.Key, g => g.Count());

            var topCategory = tickets
                .Where(t => t.AIAnalysis != null)
                .GroupBy(t => t.AIAnalysis!.Category)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var negativeTickets = tickets
                .Count(t => t.AIAnalysis != null && t.AIAnalysis.SentimentLabel == "Negative");

            var negativeRate = tickets.Any(t => t.AIAnalysis != null) 
                ? (int)Math.Round((double)negativeTickets / tickets.Count(t => t.AIAnalysis != null) * 100) 
                : 0;

            return new AnalyticsSummary
            {
                TotalTickets = tickets.Count,
                NegativeSentimentRate = negativeRate,
                AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2),
                TopIssueCategory = topCategory,
                SentimentDistribution = sentimentDistribution
            };
        }

        public async Task<IEnumerable<CategoryDistribution>> GetCategoryDistributionAsync()
        {
            return await _context.AIAnalyses
                .GroupBy(a => a.Category)
                .Select(g => new CategoryDistribution
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();
        }

        public async Task<IEnumerable<SentimentTrend>> GetSentimentTrendAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var trendData = await _context.AIAnalyses
                .Where(a => a.ProcessedAt >= thirtyDaysAgo)
                .Include(a => a.Ticket)
                .GroupBy(a => a.ProcessedAt.Date)
                .Select(g => new SentimentTrend
                {
                    Date = g.Key,
                    AverageSentiment = g.Average(a => (double)a.SentimentScore),
                    TotalTickets = g.Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return trendData;
        }
    }
}