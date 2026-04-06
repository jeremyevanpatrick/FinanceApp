using FinanceApp2.Shared.Enums;
using FinanceApp2.Shared.Helpers;

namespace FinanceApp2.Web.Helpers
{
    public class StatusMessageHelper
    {
        private readonly Func<Task> _onStateChanged;

        public List<UIStatusMessage> StatusMessages = new List<UIStatusMessage>();

        public StatusMessageHelper(Func<Task> onStateChanged)
        {
            _onStateChanged = onStateChanged;
        }

        public void ShowSuccess(string message)
        {
            ShowStatuses(new List<UIStatusMessage> { new UIStatusMessage(message, UIStatus.Success) });
        }

        public void ShowError(string message)
        {
            ShowStatuses(new List<UIStatusMessage> { new UIStatusMessage(message, UIStatus.Error) });
        }

        public void ShowInfo(string message)
        {
            ShowStatuses(new List<UIStatusMessage> { new UIStatusMessage(message, UIStatus.Information) });
        }

        public async void ShowStatuses(List<UIStatusMessage> messages)
        {
            StatusMessages = messages;
            await _onStateChanged();
        }

        public async void ClearStatuses()
        {
            StatusMessages.Clear();
            await _onStateChanged();
        }
    }
}
