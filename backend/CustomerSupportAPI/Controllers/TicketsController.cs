using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerSupportAPI.Data;
using CustomerSupportAPI.Models;
using System.Text.Json;

namespace CustomerSupportAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketsController> _logger;
        private readonly HttpClient _httpClient;

        public TicketsController(
            ApplicationDbContext context, 
            ILogger<TicketsController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:8000");
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Tickets
                    .Include(t => t.Customer)
                    .Include(t => t.AIAnalysis)
                    .OrderByDescending(t => t.CreatedAt);

                var totalCount = await query.CountAsync();
                var tickets = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTO to avoid circular references
                var ticketDtos = tickets.Select(t => new TicketDto
                {
                    TicketID = t.TicketID,
                    CustomerID = t.CustomerID,
                    Subject = t.Subject,
                    Body = t.Body,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Customer = t.Customer != null ? new CustomerDto
                    {
                        CustomerID = t.Customer.CustomerID,
                        Name = t.Customer.Name,
                        Email = t.Customer.Email,
                        Company = t.Customer.Company
                    } : null,
                    AIAnalysis = t.AIAnalysis != null ? new AIAnalysisDto
                    {
                        AnalysisID = t.AIAnalysis.AnalysisID,
                        SentimentScore = t.AIAnalysis.SentimentScore,
                        SentimentLabel = t.AIAnalysis.SentimentLabel,
                        Category = t.AIAnalysis.Category,
                        Keywords = t.AIAnalysis.Keywords,
                        ProcessedAt = t.AIAnalysis.ProcessedAt
                    } : null
                });

                return Ok(new
                {
                    data = ticketDtos,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets");
                return StatusCode(500, new { error = new { message = "Error retrieving tickets", details = ex.Message } });
            }
        }

        // GET: api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Customer)
                    .Include(t => t.AIAnalysis)
                    .FirstOrDefaultAsync(t => t.TicketID == id);

                if (ticket == null)
                {
                    return NotFound(new { error = new { message = $"Ticket with ID {id} not found" } });
                }

                // Map to DTO
                var ticketDto = new TicketDto
                {
                    TicketID = ticket.TicketID,
                    CustomerID = ticket.CustomerID,
                    Subject = ticket.Subject,
                    Body = ticket.Body,
                    Status = ticket.Status,
                    Priority = ticket.Priority,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    Customer = ticket.Customer != null ? new CustomerDto
                    {
                        CustomerID = ticket.Customer.CustomerID,
                        Name = ticket.Customer.Name,
                        Email = ticket.Customer.Email,
                        Company = ticket.Customer.Company
                    } : null,
                    AIAnalysis = ticket.AIAnalysis != null ? new AIAnalysisDto
                    {
                        AnalysisID = ticket.AIAnalysis.AnalysisID,
                        SentimentScore = ticket.AIAnalysis.SentimentScore,
                        SentimentLabel = ticket.AIAnalysis.SentimentLabel,
                        Category = ticket.AIAnalysis.Category,
                        Keywords = ticket.AIAnalysis.Keywords,
                        ProcessedAt = ticket.AIAnalysis.ProcessedAt
                    } : null
                };

                return Ok(ticketDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ticket {id}");
                return StatusCode(500, new { error = new { message = "Error retrieving ticket", details = ex.Message } });
            }
        }
        // POST: api/tickets/upload
[HttpPost("upload")]
public async Task<IActionResult> UploadTicket([FromBody] UploadTicketRequest request)
{
    try
    {
        // Validate input
        if (string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Body))
        {
            return BadRequest(new { error = new { message = "Subject and Body are required" } });
        }

        // Validate customer exists
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null)
        {
            return BadRequest(new { error = new { message = $"Customer with ID {request.CustomerId} not found" } });
        }

        // Create ticket
        var ticket = new Ticket
        {
            CustomerID = request.CustomerId,
            Subject = request.Subject,
            Body = request.Body,
            Status = "Open",
            Priority = request.Priority ?? "Medium",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Ticket {ticket.TicketID} created successfully");

        // Call AI Service for analysis (fire and forget with error handling)
        _ = Task.Run(async () =>
        {
            try
            {
                await AnalyzeTicketWithAI(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AI analysis failed for ticket {ticket.TicketID}");
            }
        });

        // Return DTO
        var ticketDto = new TicketDto
        {
            TicketID = ticket.TicketID,
            CustomerID = ticket.CustomerID,
            Subject = ticket.Subject,
            Body = ticket.Body,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Customer = new CustomerDto
            {
                CustomerID = customer.CustomerID,
                Name = customer.Name,
                Email = customer.Email,
                Company = customer.Company
            }
        };

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.TicketID }, ticketDto);
    }
    catch (DbUpdateException dbEx)
    {
        _logger.LogError(dbEx, "Database error uploading ticket");
        return StatusCode(500, new { error = new { message = "Database error occurred", details = dbEx.InnerException?.Message ?? dbEx.Message } });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading ticket");
        return StatusCode(500, new { error = new { message = "Error uploading ticket", details = ex.Message } });
    }
}

        // GET: api/tickets/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var tickets = await _context.Tickets
                    .Where(t => t.CreatedAt >= thirtyDaysAgo)
                    .Include(t => t.AIAnalysis)
                    .ToListAsync();

                var totalTickets = tickets.Count;

                // Sentiment distribution
                var sentimentDistribution = tickets
                    .Where(t => t.AIAnalysis != null)
                    .GroupBy(t => t.AIAnalysis!.SentimentLabel)
                    .Select(g => new { label = g.Key, count = g.Count() })
                    .ToList();

                // Category distribution
                var categoryDistribution = tickets
                    .Where(t => t.AIAnalysis != null)
                    .GroupBy(t => t.AIAnalysis!.Category)
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .OrderByDescending(g => g.count)
                    .ToList();

                // Average sentiment
                var avgSentiment = tickets
                    .Where(t => t.AIAnalysis != null)
                    .Select(t => (double)t.AIAnalysis!.SentimentScore)
                    .DefaultIfEmpty(0)
                    .Average();

                // Negative sentiment rate
                var negativeCount = tickets
                    .Count(t => t.AIAnalysis != null && t.AIAnalysis.SentimentLabel == "Negative");
                var totalWithSentiment = tickets.Count(t => t.AIAnalysis != null);
                var negativeRate = totalWithSentiment > 0
                    ? Math.Round((double)negativeCount / totalWithSentiment * 100, 2)
                    : 0;

                // Top category
                var topCategory = categoryDistribution.FirstOrDefault()?.category ?? "N/A";

                // Sentiment trend (group by date)
                var sentimentTrend = tickets
                    .Where(t => t.AIAnalysis != null)
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        averageSentiment = g.Average(t => (double)t.AIAnalysis!.SentimentScore),
                        totalTickets = g.Count()
                    })
                    .OrderBy(t => t.date)
                    .ToList();

                return Ok(new
                {
                    summary = new
                    {
                        totalTickets,
                        negativeSentimentRate = negativeRate,
                        averageResolutionTimeHours = 0,
                        topIssueCategory = topCategory,
                        sentimentDistribution = sentimentDistribution.ToDictionary(x => x.label, x => x.count)
                    },
                    categoryDistribution = categoryDistribution,
                    sentimentTrend = sentimentTrend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { error = new { message = "Error retrieving analytics", details = ex.Message } });
            }
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketRequest request)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return NotFound(new { error = new { message = $"Ticket with ID {id} not found" } });
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    ticket.Status = request.Status;
                }

                if (!string.IsNullOrEmpty(request.Priority))
                {
                    ticket.Priority = request.Priority;
                }

                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ticket {id}");
                return StatusCode(500, new { error = new { message = "Error updating ticket", details = ex.Message } });
            }
        }

      private async Task AnalyzeTicketWithAI(Ticket ticket)
{
    try
    {
        // Check if AI service is available
        try
        {
            var healthCheck = await _httpClient.GetAsync("/health");
            if (!healthCheck.IsSuccessStatusCode)
            {
                _logger.LogWarning($"AI service health check failed: {healthCheck.StatusCode}");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"AI service not available: {ex.Message}");
            return;
        }

        var request = new
        {
            ticket_id = ticket.TicketID,
            subject = ticket.Subject,
            body = ticket.Body
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/analyze", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var analysis = JsonSerializer.Deserialize<AIAnalysisResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (analysis != null)
            {
                var aiAnalysis = new AIAnalysis
                {
                    TicketID = ticket.TicketID,
                    SentimentScore = (decimal)analysis.SentimentScore,
                    SentimentLabel = analysis.SentimentLabel,
                    Category = analysis.Category,
                    Keywords = analysis.Keywords,
                    ProcessedAt = DateTime.UtcNow
                };

                _context.AIAnalyses.Add(aiAnalysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"AI analysis saved for ticket {ticket.TicketID}");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"AI service returned {response.StatusCode} for ticket {ticket.TicketID}: {errorContent}");
        }
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, $"HTTP error calling AI service for ticket {ticket.TicketID}");
    }
    catch (TaskCanceledException ex)
    {
        _logger.LogError(ex, $"Timeout calling AI service for ticket {ticket.TicketID}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to analyze ticket {ticket.TicketID}");
    }
}
    }

    // DTOs to prevent circular references
    public class TicketDto
    {
        public int TicketID { get; set; }
        public int CustomerID { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CustomerDto? Customer { get; set; }
        public AIAnalysisDto? AIAnalysis { get; set; }
    }

    public class CustomerDto
    {
        public int CustomerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Company { get; set; }
    }

    public class AIAnalysisDto
    {
        public int AnalysisID { get; set; }
        public decimal SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Keywords { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    // Request/Response Models
    public class UploadTicketRequest
    {
        public int CustomerId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Priority { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }

    public class AIAnalysisResponse
    {
        public int TicketId { get; set; }
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Keywords { get; set; }
    }
}