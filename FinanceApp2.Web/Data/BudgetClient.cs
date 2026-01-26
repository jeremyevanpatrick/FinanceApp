using FinanceApp2.Shared.Data;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Web.Settings;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Web.Data
{
    public class BudgetClient : IBudgetClient
    {
        private readonly RequestHelper _requestHelper;
        private readonly string _apiBaseUrl;

        public BudgetClient(IOptions<ApplicationSettings> applicationSettings, IHttpClientFactory httpClientFactory, ILogger<BudgetClient> logger)
        {
            _requestHelper = new RequestHelper(httpClientFactory.CreateClient("AuthenticatedApi"), logger);
            _apiBaseUrl = applicationSettings.Value.ApiBaseUrl;
        }

        public async Task<BudgetContainer> GetBudgetAsync(int month, int year)
        {
            string requestUrl = $"{_apiBaseUrl}/budgets/getbydate?month={month}&year={year}";

            return await _requestHelper.GetRequestAsync<BudgetContainer>(requestUrl, null, 9000);
        }

        public async Task CreateBudgetAsync(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null)
        {
            string requestUrl = $"{_apiBaseUrl}/budgets/create";

            CreateBudgetRequest request = new CreateBudgetRequest()
            {
                NewBudgetMonth = newBudgetDate.Month,
                NewBudgetYear = newBudgetDate.Year,
                SourceBudgetMonth = sourceBudgetDate?.Month,
                SourceBudgetYear = sourceBudgetDate?.Year
            };
            await _requestHelper.PostRequestNoResponseAsync<CreateBudgetRequest>(requestUrl, request, null, 9000);
        }

        public async Task UpdateBudgetAsync(BudgetDto budget)
        {
            string requestUrl = $"{_apiBaseUrl}/budgets/update";

            UpdateBudgetRequest request = new UpdateBudgetRequest()
            {
                Budget = budget
            };
            await _requestHelper.PostRequestNoResponseAsync<UpdateBudgetRequest>(requestUrl, request, null, 9000);
        }

        public async Task DeleteBudgetAsync(Guid budgetId)
        {
            string requestUrl = $"{_apiBaseUrl}/budgets/delete";

            DeleteBudgetRequest request = new DeleteBudgetRequest()
            {
                BudgetId = budgetId
            };
            await _requestHelper.PostRequestNoResponseAsync<DeleteBudgetRequest>(requestUrl, request, null, 9000);
        }
    }
}
