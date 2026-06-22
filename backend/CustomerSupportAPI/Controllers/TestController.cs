using Microsoft.AspNetCore.Mvc;

namespace CustomerSupportAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API is working!",
                timestamp = DateTime.UtcNow,
                status = "running"
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "CustomerSupportAPI"
            });
        }

        [HttpGet("db")]
        public IActionResult TestDatabase([FromServices] Data.ApplicationDbContext context)
        {
            try
            {
                var canConnect = context.Database.CanConnect();
                return Ok(new
                {
                    database = canConnect ? "connected" : "disconnected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}