using FinanceApp2.Api.Errors;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Shared.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class BudgetsController : ControllerBaseExtended
    {
        private readonly ILogger<BudgetsController> _logger;
        private readonly IBudgetAppService _budgetAppService;

        public BudgetsController(
            ILogger<BudgetsController> logger,
            IBudgetAppService budgetAppService)
        {
            _logger = logger;
            _budgetAppService = budgetAppService;
        }

        [HttpGet("{year:int}/{month:int}")]
        public async Task<ActionResult<BudgetContainer>> Get(int year, int month)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                BudgetContainer budgetContainer = await _budgetAppService.GetByDateAsync(userId, month, year);

                return Ok(budgetContainer);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.GetByDateUnexpected, ex, "Unexpected error while getting budget by date", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", month.ToString() },
                    { "Year", year.ToString() }
                });
                return Problem500();
            }
        }

        [HttpPost]
        public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                var budget = await _budgetAppService.CreateAsync(userId, request.NewBudgetMonth, request.NewBudgetYear, request.SourceBudgetMonth, request.SourceBudgetYear);

                return CreatedAtAction(
                    nameof(Get),
                    new { year = request.NewBudgetYear, month = request.NewBudgetMonth },
                    budget);
            }
            catch (BudgetConflictException ex)
            {
                return Problem409(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (NotFoundException ex)
            {
                return Problem404(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.CreateBudgetUnexpected, ex, "Unexpected error while creating budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", request.NewBudgetMonth.ToString() },
                    { "Year", request.NewBudgetYear.ToString() }
                });
                return Problem500();
            }
        }

        [HttpPatch("{year:int}/{month:int}")]
        public async Task<IActionResult> Update(int year, int month, [FromBody] UpdateBudgetRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                await _budgetAppService.UpdateAsync(userId, month, year, request.Income, request.Groups);

                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return Problem404(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.UpdateBudgetUnexpected, ex, "Unexpected error while updating budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", month.ToString() },
                    { "Year", year.ToString() }
                });
                return Problem500();
            }
        }

        [HttpDelete("{year:int}/{month:int}")]
        public async Task<IActionResult> Delete(int year, int month)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                await _budgetAppService.DeleteAsync(userId, year, month);

                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return Problem404(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.DeleteBudgetUnexpected, ex, "Unexpected error while deleting budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", month.ToString() },
                    { "Year", year.ToString() }
                });
                return Problem500();
            }
        }
    }
}
