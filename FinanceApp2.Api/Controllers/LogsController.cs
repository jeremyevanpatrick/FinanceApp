using FinanceApp2.Api.Services;
using FinanceApp2.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LoggingService _loggingService;

        public LogsController(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpPost("logerror")]
        public async Task<IActionResult> LogError([FromBody] Error error)
        {
            _loggingService.LogError(error);
            return Ok();
        }
    }
}
