using CustomerSupportAPI.Models;

namespace CustomerSupportAPI.Services
{
    public interface IAIService
    {
        Task AnalyzeTicketAsync(Ticket ticket);
    }
}