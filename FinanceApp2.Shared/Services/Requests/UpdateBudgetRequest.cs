using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Shared.Services.Requests
{
    public class UpdateBudgetRequest
    {
        public int? Income { get; set; } = 0;
        public List<GroupDto> Groups { get; set; } = new List<GroupDto>();
    }
}