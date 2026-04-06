using Blazored.SessionStorage;
using FinanceApp2.Shared.Enums;
using FinanceApp2.Shared.Helpers;

namespace FinanceApp2.Web.Services
{
    public class NavigationMessageService
    {
        private readonly ISessionStorageService _sessionStorage;

        public NavigationMessageService(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task AddSuccess(string message)
        {
            await AddStatus(message, UIStatus.Success);
        }

        public async Task AddError(string message)
        {
            await AddStatus(message, UIStatus.Error);
        }

        private async Task AddStatus(string message, UIStatus status)
        {
            var statusMessages = await _sessionStorage.GetItemAsync<List<UIStatusMessage>>("StatusMessages") ?? new List<UIStatusMessage>();
            statusMessages.Add(new UIStatusMessage(message, status));
            await _sessionStorage.SetItemAsync("StatusMessages", statusMessages);
        }

        public async Task<List<UIStatusMessage>> ConsumeAll()
        {
            var statusMessages = await _sessionStorage.GetItemAsync<List<UIStatusMessage>>("StatusMessages") ?? new List<UIStatusMessage>();
            await _sessionStorage.RemoveItemAsync("StatusMessages");
            return statusMessages;
        }

    }

}
