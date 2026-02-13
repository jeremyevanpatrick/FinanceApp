using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp2.Api.Models
{
    public class Item
    {
        [Key]
        public Guid ItemId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public string ItemName { get; set; } = null!;

        public int? Spent { get; set; }
        public int? Budgeted { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GroupId))]
        public Group Group { get; set; } = null!;

        // Sync metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
