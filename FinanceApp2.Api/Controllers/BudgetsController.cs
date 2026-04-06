using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Extensions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Models;
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
        private readonly BudgetsLinkHelper _budgetsLinkHelper;
        private readonly IBudgetAppService _budgetAppService;

        public BudgetsController(
            ILogger<BudgetsController> logger,
            BudgetsLinkHelper budgetsLinkHelper,
            IBudgetAppService budgetAppService)
        {
            _logger = logger;
            _budgetsLinkHelper = budgetsLinkHelper;
            _budgetAppService = budgetAppService;
        }

        [HttpGet("{year:int}/{month:int}")]
        public async Task<ActionResult<BudgetContainerDto>> Get([FromRoute] int year, [FromRoute] int month)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(BudgetsController), nameof(Get), correlationId))
            {
                Guid userId = Guid.Empty;

                try
                {
                    userId = User.GetUserId();

                    BudgetContainerDto budgetContainer = await _budgetAppService.GetByDateAsync(userId, month, year);

                    budgetContainer.Links = _budgetsLinkHelper.GetLinksForBudgetsGet(year, month, budgetContainer.Budget != null);

                    return Ok(budgetContainer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while getting budget by date. ErrorCode: {ErrorCode}, UserId: {UserId}, Month: {Month}, Year: {Year}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId,
                        month,
                        year);
                    return Problem500();
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(BudgetsController), nameof(Get), correlationId))
            {
                Guid userId = Guid.Empty;

                try
                {
                    userId = User.GetUserId();

                    var budget = await _budgetAppService.CreateAsync(userId, request.NewBudgetMonth, request.NewBudgetYear, request.SourceBudgetMonth, request.SourceBudgetYear);

                    budget.Links = _budgetsLinkHelper.GetLinksForBudgetsCreate(budget.Year, budget.Month);

                    return CreatedAtAction(
                        nameof(Get),
                        new { year = request.NewBudgetYear, month = request.NewBudgetMonth },
                        budget);
                }
                catch (BudgetConflictException ex)
                {
                    var links = new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsGetSelf(request.NewBudgetYear, request.NewBudgetMonth),
                        _budgetsLinkHelper.BudgetsUpdate(request.NewBudgetYear, request.NewBudgetMonth)
                    };
                    return Problem409(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS, links);
                }
                catch (NotFoundException ex)
                {
                    var links = new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsCreate()
                    };
                    return Problem404(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while creating budget. ErrorCode: {ErrorCode}, UserId: {UserId}, Month: {Month}, Year: {Year}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId,
                        request.NewBudgetMonth,
                        request.NewBudgetYear);
                    return Problem500();
                }
            }
        }

        [HttpPatch("{year:int}/{month:int}")]
        public async Task<ActionResult<List<Link>>> Update([FromRoute] int year, [FromRoute] int month, [FromBody] UpdateBudgetRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(BudgetsController), nameof(Get), correlationId))
            {
                Guid userId = Guid.Empty;

                try
                {
                    userId = User.GetUserId();

                    await _budgetAppService.UpdateAsync(userId, month, year, request.Income, request.Groups);

                    return Ok(new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsGetSelf(year, month),
                        _budgetsLinkHelper.BudgetsUpdate(year, month),
                        _budgetsLinkHelper.BudgetsDelete(year, month)
                    });
                }
                catch (NotFoundException ex)
                {
                    var links = new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsCreate()
                    };
                    return Problem404(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while updating budget. ErrorCode: {ErrorCode}, UserId: {UserId}, Month: {Month}, Year: {Year}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId,
                        month,
                        year);
                    return Problem500();
                }
            }
        }

        [HttpDelete("{year:int}/{month:int}")]
        public async Task<ActionResult<List<Link>>> Delete([FromRoute] int year, [FromRoute] int month)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(BudgetsController), nameof(Get), correlationId))
            {
                Guid userId = Guid.Empty;

                try
                {
                    userId = User.GetUserId();

                    await _budgetAppService.DeleteAsync(userId, year, month);

                    return Ok(new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsCreate()
                    });
                }
                catch (NotFoundException ex)
                {
                    var links = new List<Link>
                    {
                        _budgetsLinkHelper.BudgetsCreate()
                    };
                    return Problem404(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS, links);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while deleting budget. ErrorCode: {ErrorCode}, UserId: {UserId}, Month: {Month}, Year: {Year}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId,
                        month,
                        year);
                    return Problem500();
                }
            }
        }
    }
}
