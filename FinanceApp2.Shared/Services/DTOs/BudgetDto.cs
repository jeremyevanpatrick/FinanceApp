using FinanceApp2.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceApp2.Shared.Services.DTOs
{
    public class BudgetDto : Resource
    {
        [Required]
        public Guid BudgetId { get; set; }
        [Required]
        public int Month { get; set; }
        [Required]
        public int Year { get; set; }
        public int? Income { get; set; } = 0;
        [Required]
        public Guid UserId { get; set; }
        public List<GroupDto> Groups { get; set; } = new List<GroupDto>();

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public int BudgetTotal
        {
            get
            {
                return Groups?.Where(x => !x.IsDeleted)?.Sum(x => x.GroupTotal) ?? 0;
            }
        }

        [JsonIgnore]
        public int BalanceTotal
        {
            get
            {
                return (Income ?? 0) - BudgetTotal;
            }
        }
    }
}