using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Web.Helpers;
using Microsoft.AspNetCore.Components;

namespace FinanceApp2.Web.Pages
{
    public partial class BudgetPage
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int? Month { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public int? Year { get; set; }

        private BudgetContainer budgetContainer = new BudgetContainer();
        private List<GroupDto> orderedGroups => budgetContainer?.Budget?.Groups
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.Order)
            .ToList() ?? new List<GroupDto>();

        private DateOnly currentBudgetDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
        private DateOnly previousBudgetDate
        {
            get
            {
                return currentBudgetDate.AddMonths(-1);
            }
        }
        private DateOnly nextBudgetDate
        {
            get
            {
                return currentBudgetDate.AddMonths(1);
            }
        }

        private bool isInitialBudgetLoaded = false;
        private bool isEnterMode = false;
        private bool showConfirmDeleteBudget = false;
        private string newGroupName = "";

        private StatusMessageHelper statusMessageHelper = default!;

        protected override async Task OnParametersSetAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!(authState.User.Identity?.IsAuthenticated ?? false))
            {
                return;
            }

            statusMessageHelper = new StatusMessageHelper(() => InvokeAsync(StateHasChanged));
            statusMessageHelper.ShowStatuses(MessageService.ConsumeAll());

            if (Month != null && Month > 0 && Month < 13 &&
                Year != null && Year > 0 && Year < 10000)
            {
                currentBudgetDate = new DateOnly((int)Year, (int)Month, 1);
            }

            await LoadBudget();
        }

        private async Task LoadBudget()
        {
            try
            {
                isInitialBudgetLoaded = false;
                budgetContainer = await BudgetClient.GetBudgetAsync(currentBudgetDate.Month, currentBudgetDate.Year);
                isInitialBudgetLoaded = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                statusMessageHelper.ShowError("Failed to load budget, please try again.");
            }
        }

        private void HandleUpdateCalculation()
        {
            StateHasChanged();
        }

        private void HandleDeleteGroup(GroupDto group)
        {
            group.IsDeleted = true;
            group.ModifiedAt = DateTime.UtcNow;
            StateHasChanged();
        }

        private void HandleMoveUpGroup(GroupDto group)
        {
            if (budgetContainer.Budget != null)
            {
                //swap the position of the current group with the position of the preceding group
                int movingGroupOrder = group.Order;
                GroupDto precedingGroup = budgetContainer.Budget.Groups.OrderBy(x => x.Order).Last(x => x.Order < group.Order);
                group.Order = precedingGroup.Order;
                precedingGroup.Order = movingGroupOrder;
                group.ModifiedAt = DateTime.UtcNow;
                precedingGroup.ModifiedAt = DateTime.UtcNow;
                StateHasChanged();
            }
        }

        private string GetMonthYearDisplay()
        {
            return currentBudgetDate.ToString("MMMM yyyy");
        }

        private void NavigateToPreviousMonth()
        {
            DisableEnterMode();
            NavManager.NavigateTo($"/budgets?month={previousBudgetDate.Month}&year={previousBudgetDate.Year}");
        }

        private void NavigateToNextMonth()
        {
            DisableEnterMode();
            NavManager.NavigateTo($"/budgets?month={nextBudgetDate.Month}&year={nextBudgetDate.Year}");
        }

        private void EnableEnterMode()
        {
            isEnterMode = true;
        }

        private void DisableEnterMode()
        {
            isEnterMode = false;
            newGroupName = string.Empty;
        }

        private async Task OnSaveChanges()
        {
            try
            {
                if (budgetContainer.Budget != null)
                {
                    budgetContainer.Budget.ModifiedAt = DateTime.UtcNow;

                    await BudgetClient.UpdateBudgetAsync(budgetContainer.Budget);

                    statusMessageHelper.ShowSuccess("Budget successfully updated.");
                }

                DisableEnterMode();
            }
            catch (Exception ex)
            {
                statusMessageHelper.ShowError("Failed to save changes, please try again.");
            }
        }

        private async Task OnCancelEdit()
        {
            DisableEnterMode();
            await LoadBudget();
        }

        private void HandleAddGroup()
        {
            if (budgetContainer.Budget != null && !string.IsNullOrWhiteSpace(newGroupName))
            {
                int newOrder = orderedGroups.LastOrDefault()?.Order + 1 ?? 1;
                var now = DateTime.UtcNow;
                var newGroup = new GroupDto
                {
                    GroupId = Guid.NewGuid(),
                    GroupName = newGroupName,
                    BudgetId = budgetContainer.Budget.BudgetId,
                    Order = newOrder,
                    CreatedAt = now,
                    ModifiedAt = now
                };

                budgetContainer.Budget.Groups.Add(newGroup);
                newGroupName = string.Empty;

                StateHasChanged();
            }
        }

        private async Task ConfirmDeleteBudgetClicked()
        {
            try
            {
                if (budgetContainer.Budget != null)
                {
                    await BudgetClient.DeleteBudgetAsync(budgetContainer.Budget.BudgetId);

                    statusMessageHelper.ShowSuccess("Budget successfully deleted.");
                }

                showConfirmDeleteBudget = false;
                DisableEnterMode();
                await LoadBudget();
            }
            catch (Exception ex)
            {
                statusMessageHelper.ShowError("Failed to delete budget, please try again.");
            }
        }

        private async Task OnCreateBudget(DateOnly newBudgetDate, DateOnly? sourceBudgetDate = null)
        {
            try
            {
                await BudgetClient.CreateBudgetAsync(newBudgetDate, sourceBudgetDate);
                string successMessage = "Budget created successfully!";
                if (currentBudgetDate == newBudgetDate)
                {
                    statusMessageHelper.ShowSuccess(successMessage);
                    await LoadBudget();
                }
                else
                {
                    MessageService.AddSuccess("Budget created successfully!");
                    NavManager.NavigateTo($"/budgets?month={newBudgetDate.Month}&year={newBudgetDate.Year}");
                }
            }
            catch (Exception ex)
            {
                statusMessageHelper.ShowError("Failed to create new budget, please try again.");
            }
        }

        private async Task OnEditAccount()
        {
            NavManager.NavigateTo("/account");
        }
    }
}