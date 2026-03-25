using FinanceApp2.Api.Services;
using FinanceApp2.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("error-logs")]
    public class LogsController : ControllerBase
    {
        private readonly ErrorLogQueue _errorLogQueue;

        public LogsController(ErrorLogQueue errorLogQueue)
        {
            _errorLogQueue = errorLogQueue;
        }

        [HttpPost]
        public async Task<IActionResult> LogError([FromBody] Error error)
        {
            _errorLogQueue.Enqueue(error);
            return Accepted();
        }
    }
}
