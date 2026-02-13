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

        public Task<BaseResult<BudgetContainer?>> GetBudgetAsync(int month, int year) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/getbydate?month={month}&year={year}";

                return await _requestHelper.GetAsync<BudgetContainer>(requestUrl, false, 9000);
            });

        public Task<BaseResult> CreateBudgetAsync(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/create";

                CreateBudgetRequest request = new CreateBudgetRequest()
                {
                    NewBudgetMonth = newBudgetDate.Month,
                    NewBudgetYear = newBudgetDate.Year,
                    SourceBudgetMonth = sourceBudgetDate?.Month,
                    SourceBudgetYear = sourceBudgetDate?.Year
                };
                await _requestHelper.PostAsync<CreateBudgetRequest>(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> UpdateBudgetAsync(BudgetDto budget) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/update";

                UpdateBudgetRequest request = new UpdateBudgetRequest()
                {
                    Budget = budget
                };
                await _requestHelper.PostAsync<UpdateBudgetRequest>(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> DeleteBudgetAsync(Guid budgetId) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_apiBaseUrl}/budgets/delete";

                DeleteBudgetRequest request = new DeleteBudgetRequest()
                {
                    BudgetId = budgetId
                };
                await _requestHelper.PostAsync<DeleteBudgetRequest>(requestUrl, request, false, 9000);
            });
    }
}
