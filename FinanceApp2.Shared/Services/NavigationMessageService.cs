using FinanceApp2.Shared.Helpers;

namespace FinanceApp2.Shared.Services
{
    public class NavigationMessageService
    {
        private readonly List<string> _successMessages = new List<string>();
        private readonly List<string> _errorMessages = new List<string>();

        public void AddSuccess(string message)
        {
            _successMessages.Add(message);
        }

        public List<string> ConsumeSuccess()
        {
            var messages = new List<string>(_successMessages);
            _successMessages.Clear();
            return messages;
        }

        public void AddError(string message)
        {
            _errorMessages.Add(message);
        }

        public List<string> ConsumeErrors()
        {
            var messages = new List<string>(_errorMessages);
            _errorMessages.Clear();
            return messages;
        }

        public List<(string, UIStatus)> ConsumeAll()
        {
            List<(string, UIStatus)> messages = new List<(string, UIStatus)>();

            List<string> errorMessages = ConsumeErrors();
            messages.AddRange(errorMessages.Select(x => (x, UIStatus.Error)));

            List<string> successMessages = ConsumeSuccess();
            messages.AddRange(successMessages.Select(x => (x, UIStatus.Success)));

            return messages;
        }

    }

}
