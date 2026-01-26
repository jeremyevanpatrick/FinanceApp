using FinanceApp2.Shared.Helpers;

namespace FinanceApp2.Web.Helpers
{
    public class StatusMessageHelper
    {
        private readonly Func<Task> _onStateChanged;

        public List<(string, UIStatus)> StatusMessages = new List<(string, UIStatus)>();

        public StatusMessageHelper(Func<Task> onStateChanged)
        {
            _onStateChanged = onStateChanged;
        }

        public void ShowSuccess(string message)
        {
            ShowStatuses(new List<(string, UIStatus)> { (message, UIStatus.Success) });
        }

        public void ShowError(string message)
        {
            ShowStatuses(new List<(string, UIStatus)> { (message, UIStatus.Error) });
        }

        public void ShowInfo(string message)
        {
            ShowStatuses(new List<(string, UIStatus)> { (message, UIStatus.Information) });
        }

        public async void ShowStatuses(List<(string, UIStatus)> messages)
        {
            StatusMessages = messages;
            await _onStateChanged();

            await Task.Delay(3000);

            StatusMessages.Clear();
            await _onStateChanged();
        }
    }
}
