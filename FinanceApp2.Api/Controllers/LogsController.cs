using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Queues;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("application-logs")]
    public class LogsController : ControllerBase
    {
        private readonly ILogProcessorQueue _logProcessorQueue;

        public LogsController(ILogProcessorQueue logProcessorQueue)
        {
            _logProcessorQueue = logProcessorQueue;
        }

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] ApplicationLog applicationLog)
        {
            _logProcessorQueue.Enqueue(applicationLog);
            return Accepted();
        }

        [HttpPost("batch")]
        public async Task<IActionResult> LogBatch([FromBody] List<ApplicationLog> applicationLogs)
        {
            if (applicationLogs != null)
            {
                foreach (var applicationLog in applicationLogs)
                {
                    _logProcessorQueue.Enqueue(applicationLog);
                }
            }
            return Accepted();
        }
    }
}
