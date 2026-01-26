using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp2.Shared.Models
{
    public class Group
    {
        [Key]
        public Guid GroupId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BudgetId { get; set; }

        [Required]
        public string GroupName { get; set; }

        [Required]
        public int Order { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BudgetId))]
        public Budget Budget { get; set; } = null!;
        public List<Item> Items { get; set; } = new List<Item>();

        // Sync metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
