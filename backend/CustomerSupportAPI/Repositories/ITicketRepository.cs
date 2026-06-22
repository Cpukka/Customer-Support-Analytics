using CustomerSupportAPI.Models;

namespace CustomerSupportAPI.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetTicketsAsync(int page, int pageSize);
        Task<int> GetTotalTicketCountAsync();
        Task UpdateTicketAsync(Ticket ticket);
        Task<AnalyticsSummary> GetAnalyticsSummaryAsync();
        Task<IEnumerable<CategoryDistribution>> GetCategoryDistributionAsync();
        Task<IEnumerable<SentimentTrend>> GetSentimentTrendAsync();
    }

    public class AnalyticsSummary
    {
        public int TotalTickets { get; set; }
        public int NegativeSentimentRate { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public string TopIssueCategory { get; set; } = string.Empty;
        public Dictionary<string, int> SentimentDistribution { get; set; } = new();
    }

    public class CategoryDistribution
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SentimentTrend
    {
        public DateTime Date { get; set; }
        public double AverageSentiment { get; set; }
        public int TotalTickets { get; set; }
    }
}