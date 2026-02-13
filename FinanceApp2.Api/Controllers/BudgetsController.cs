using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services;
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
        private readonly IBudgetDbService _budgetService;

        public BudgetsController(ILogger<BudgetsController> logger, IBudgetDbService budgetService)
        {
            _logger = logger;
            _budgetService = budgetService;
        }

        [HttpGet("getbydate")]
        public async Task<ActionResult<BudgetContainer>> GetByDate([FromQuery] GetByDateRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                DateOnly requestedDate = new DateOnly(request.Year, request.Month, 1);
                DateOnly previousMonthDate = requestedDate.AddMonths(-1);
                DateOnly nextMonthDate = requestedDate.AddMonths(1);

                Budget? budget = await _budgetService.GetByDate(userId, request.Month, request.Year);
                bool hasPreviousMonth = await _budgetService.GetExistsByDate(userId, previousMonthDate.Month, previousMonthDate.Year);
                bool hasNextMonth = await _budgetService.GetExistsByDate(userId, nextMonthDate.Month, nextMonthDate.Year);

                BudgetContainer budgetContainer = new BudgetContainer()
                {
                    Budget = BudgetMapper.ToDto(budget),
                    HasPreviousMonth = hasPreviousMonth,
                    HasNextMonth = hasNextMonth
                };

                return Ok(budgetContainer);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.GetByDateUnexpected, ex, "Unexpected error while getting budget by date", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", request.Month.ToString() },
                    { "Year", request.Year.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Budget newBudget = new Budget
                {
                    Month = request.NewBudgetMonth,
                    Year = request.NewBudgetYear,
                    UserId = userId,
                    Income = 0,
                    Groups = new List<Group>(),
                    ModifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                if (request.SourceBudgetMonth != null && request.SourceBudgetYear != null)
                {
                    //if source fields are included in the request, clone contents from the source budget
                    Budget? sourceBudget = await _budgetService.GetByDate(userId, (int)request.SourceBudgetMonth, (int)request.SourceBudgetYear);

                    if (sourceBudget == null)
                    {
                        return Problem400("Source budget not found. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                    }

                    newBudget.Groups = sourceBudget.Groups.Select(g =>
                    {
                        Group newGroup = new Group
                        {
                            BudgetId = newBudget.BudgetId,
                            Budget = newBudget,
                            GroupName = g.GroupName,
                            Order = g.Order,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };

                        newGroup.Items = g.Items.Select(i => new Item
                        {
                            GroupId = newGroup.GroupId,
                            ItemName = i.ItemName,
                            Budgeted = i.Budgeted,
                            Group = newGroup,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        }).ToList();

                        return newGroup;
                    }).ToList();
                }

                await _budgetService.CreateAsync(newBudget);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.CreateBudgetUnexpected, ex, "Unexpected error while creating budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "Month", request.NewBudgetMonth.ToString() },
                    { "Year", request.NewBudgetYear.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateBudgetRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Budget? updatedBudget = BudgetMapper.ToEntity(request.Budget);

                Budget? existingBudget = await _budgetService.GetById(updatedBudget.BudgetId, true);
                
                if (existingBudget == null)
                {
                    return Problem400("Invalid budgetId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                await _budgetService.UpdateAsync(existingBudget, updatedBudget);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.UpdateBudgetUnexpected, ex, "Unexpected error while updating budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "BudgetId", request.Budget.BudgetId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteBudgetRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Budget? existingBudget = await _budgetService.GetById(request.BudgetId);

                if (existingBudget == null)
                {
                    return Problem400("Invalid budgetId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                await _budgetService.DeleteAsync(existingBudget);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(BudgetErrorCodes.DeleteBudgetUnexpected, ex, "Unexpected error while deleting budget", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "BudgetId", request.BudgetId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }
    }
}
