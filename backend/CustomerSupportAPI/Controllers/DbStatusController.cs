using Microsoft.AspNetCore.Mvc;
using CustomerSupportAPI.Data;

namespace CustomerSupportAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DbStatusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DbStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new { 
                    status = canConnect ? "connected" : "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    status = "error", 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}