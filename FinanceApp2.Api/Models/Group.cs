namespace FinanceApp2.Api.Models
{
    public class Group
    {
        public Guid GroupId { get; set; } = Guid.NewGuid();

        public Guid BudgetId { get; set; }

        public required string GroupName { get; set; }

        public required int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public Budget Budget { get; set; } = null!;

        public List<Item> Items { get; set; } = new List<Item>();
    }
}
