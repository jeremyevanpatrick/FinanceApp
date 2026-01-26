using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Services.DTOs
{
    public class ItemDto
    {
        [Required]
        public Guid ItemId { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public Guid GroupId { get; set; }
        public int? Spent { get; set; }
        public int? Budgeted { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}