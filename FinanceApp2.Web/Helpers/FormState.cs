using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace FinanceApp2.Web.Helpers
{
    public sealed class FormState<TModel> where TModel : new()
    {
        public TModel Model { get; private set; }
        public EditContext EditContext { get; private set; }
        public bool IsValid { get; private set; }

        private readonly Action _onStateChanged;

        public FormState(Action onStateChanged)
        {
            _onStateChanged = onStateChanged;
            Reset();
        }

        public void Reset()
        {
            Model = new TModel();
            EditContext = new EditContext(Model);
            IsValid = false;

            EditContext.OnFieldChanged += (_, __) => Validate();
        }

        private void Validate()
        {
            var validationContext = new ValidationContext(Model);
            var results = new List<ValidationResult>();

            IsValid = Validator.TryValidateObject(
                Model,
                validationContext,
                results,
                validateAllProperties: true
            );

            _onStateChanged();
        }
    }
}
