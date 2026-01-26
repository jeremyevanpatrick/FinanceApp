using System.ComponentModel.DataAnnotations;

namespace FinanceApp2.Shared.Models
{
    public class Budget
    {
        [Key]
        public Guid BudgetId { get; set; } = Guid.NewGuid();
        [Required]
        public int Month { get; set; }
        [Required]
        public int Year { get; set; }
        public int Income { get; set; } = 0;
        [Required]
        public Guid UserId { get; set; }

        // Navigation property
        public List<Group> Groups { get; set; } = null!;

        // Sync metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
