using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerSupportAPI.Data;
using CustomerSupportAPI.Models;

namespace CustomerSupportAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ApplicationDbContext context,
            ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Customers
                    .OrderByDescending(c => c.CreatedAt);

                var totalCount = await query.CountAsync();
                var customers = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get ticket counts for each customer
                var customerIds = customers.Select(c => c.CustomerID).ToList();
                var ticketCounts = await _context.Tickets
                    .Where(t => customerIds.Contains(t.CustomerID))
                    .GroupBy(t => t.CustomerID)
                    .Select(g => new { CustomerId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

                var result = customers.Select(c => new
                {
                    c.CustomerID,
                    c.Name,
                    c.Email,
                    c.Company,
                    c.CreatedAt,
                    c.UpdatedAt,
                    TicketCount = ticketCounts.ContainsKey(c.CustomerID) ? ticketCounts[c.CustomerID] : 0
                });

                return Ok(new
                {
                    data = result,
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
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, new { error = "Error retrieving customers" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Tickets)
                    .FirstOrDefaultAsync(c => c.CustomerID == id);

                if (customer == null)
                {
                    return NotFound(new { error = $"Customer with ID {id} not found" });
                }

                return Ok(new
                {
                    customer.CustomerID,
                    customer.Name,
                    customer.Email,
                    customer.Company,
                    customer.CreatedAt,
                    customer.UpdatedAt,
                    Tickets = customer.Tickets.Select(t => new
                    {
                        t.TicketID,
                        t.Subject,
                        t.Status,
                        t.Priority,
                        t.CreatedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer {id}");
                return StatusCode(500, new { error = "Error retrieving customer" });
            }
        }

        [HttpGet("{id}/tickets")]
        public async Task<IActionResult> GetCustomerTickets(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerID == id);

                if (customer == null)
                {
                    return NotFound(new { error = $"Customer with ID {id} not found" });
                }

                var tickets = await _context.Tickets
                    .Where(t => t.CustomerID == id)
                    .Include(t => t.AIAnalysis)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Ok(new
                {
                    customerId = id,
                    customerName = customer.Name,
                    tickets = tickets.Select(t => new
                    {
                        t.TicketID,
                        t.Subject,
                        t.Status,
                        t.Priority,
                        t.CreatedAt,
                        t.UpdatedAt,
                        Sentiment = t.AIAnalysis != null ? new
                        {
                            t.AIAnalysis.SentimentLabel,
                            t.AIAnalysis.SentimentScore,
                            t.AIAnalysis.Category
                        } : null
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tickets for customer {id}");
                return StatusCode(500, new { error = "Error retrieving customer tickets" });
            }
        }
    }
}