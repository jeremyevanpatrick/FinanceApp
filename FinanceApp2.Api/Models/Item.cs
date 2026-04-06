namespace FinanceApp2.Api.Models
{
    public class Item
    {
        public Guid ItemId { get; set; } = Guid.NewGuid();

        public Guid GroupId { get; set; }

        public required string ItemName { get; set; }

        public int? Spent { get; set; }

        public int? Budgeted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public Group Group { get; set; } = null!;

    }
}
