using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Web.Helpers;
using FinanceApp2.Web.Services;
using FinanceApp2.Web.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Web.Data
{
    public class BudgetClient : BaseClient, IBudgetClient
    {
        private readonly RequestHelper _requestHelper;
        private readonly string _apiBaseUrl;

        public BudgetClient(
            IOptions<ApplicationSettings> applicationSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<BudgetClient> logger,
            NavigationManager navigationManager,
            NavigationMessageService navigationMessageService)
            : base(logger, navigationManager, navigationMessageService)
        {
            _requestHelper = new RequestHelper(httpClientFactory.CreateClient("AuthenticatedApi"));
            _apiBaseUrl = applicationSettings.Value.ApiBaseUrl;
        }

        public Task<BaseResult<BudgetContainer?>> GetBudgetAsync(int year, int month) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/{year}/{month}";

                return await _requestHelper.GetAsync<BudgetContainer>(requestUrl, false, 9000);
            });

        public Task<BaseResult<BudgetDto?>> CreateBudgetAsync(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets";

                CreateBudgetRequest request = new CreateBudgetRequest()
                {
                    NewBudgetMonth = newBudgetDate.Month,
                    NewBudgetYear = newBudgetDate.Year,
                    SourceBudgetMonth = sourceBudgetDate?.Month,
                    SourceBudgetYear = sourceBudgetDate?.Year
                };
                return await _requestHelper.PostAsync<CreateBudgetRequest, BudgetDto>(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> UpdateBudgetAsync(BudgetDto budget) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/{budget.Year}/{budget.Month}";

                UpdateBudgetRequest request = new UpdateBudgetRequest()
                {
                    Income = budget.Income,
                    Groups = budget.Groups
                };
                await _requestHelper.PatchAsync<UpdateBudgetRequest>(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> DeleteBudgetAsync(int year, int month) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/{year}/{month}";

                await _requestHelper.DeleteAsync<object>(requestUrl, null, false, 9000);
            });
    }
}
