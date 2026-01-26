using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceApp2.Shared.Services.DTOs
{
    public class GroupDto
    {
        [Required]
        public Guid GroupId { get; set; }
        [Required]
        public string GroupName { get; set; }
        [Required]
        public Guid BudgetId { get; set; }
        [Required]
        public int Order { get; set; }
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public int GroupTotal
        {
            get
            {
                return Items.Where(x => !x.IsDeleted)?.Sum(x => x.Budgeted) ?? 0;
            }
        }
    }
}