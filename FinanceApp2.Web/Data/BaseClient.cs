using FinanceApp2.Shared.Enums;
using FinanceApp2.Shared.Exceptions;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Web.Errors;
using FinanceApp2.Web.Services;
using Microsoft.AspNetCore.Components;

namespace FinanceApp2.Web.Data
{
    public abstract class BaseClient
    {
        protected ILogger<BaseClient> _logger { get; }
        protected NavigationManager _navigationManager { get; }
        protected NavigationMessageService _navigationMessageService { get; }

        protected BaseClient(
            ILogger<BaseClient> logger,
            NavigationManager navigationManager,
            NavigationMessageService navigationMessageService)
        {
            _logger = logger;
            _navigationManager = navigationManager;
            _navigationMessageService = navigationMessageService;
        }

        protected async Task<BaseResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await action();
                return new BaseResult<T>(result);
            }
            catch (HttpRecoverableError ex)
            {
                if (ex.IsUnauthorized)
                {
                    await _navigationMessageService.AddError(ex.Message);
                    _navigationManager.NavigateTo("/login", forceLoad: false);
                }
                return new BaseResult<T>(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in client. ErrorCode: {ErrorCode}",
                    WebErrorCodes.CLIENT_UNEXPECTED_ERROR);
                return new BaseResult<T>(false, WebErrorMessages.UnknownError);
            }
        }

        protected async Task<BaseResult> ExecuteAsync(Func<Task> action)
        {
            try
            {
                await action();
                return new BaseResult();
            }
            catch (HttpRecoverableError ex)
            {
                if (ex.IsUnauthorized)
                {
                    await _navigationMessageService.AddError(ex.Message);
                    _navigationManager.NavigateTo("/login", forceLoad: false);
                }
                return new BaseResult(false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in client. ErrorCode: {ErrorCode}",
                    WebErrorCodes.CLIENT_UNEXPECTED_ERROR);
                return new BaseResult(false, WebErrorMessages.UnknownError);
            }
        }
    }
}
