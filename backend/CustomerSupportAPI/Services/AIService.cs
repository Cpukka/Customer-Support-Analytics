using System.Text;
using System.Text.Json;
using CustomerSupportAPI.Models;

namespace CustomerSupportAPI.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;

        public AIService(HttpClient httpClient, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task AnalyzeTicketAsync(Ticket ticket)
        {
            try
            {
                var request = new
                {
                    ticket_id = ticket.TicketID,
                    subject = ticket.Subject,
                    body = ticket.Body
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/analyze", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Ticket {ticket.TicketID} analyzed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to analyze ticket {ticket.TicketID}");
                throw;
            }
        }
    }
}