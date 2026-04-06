namespace FinanceApp2.Api.Models
{
    public class Budget
    {
        public Guid BudgetId { get; set; } = Guid.NewGuid();
        
        public required int Month { get; set; }
        
        public required int Year { get; set; }

        public int Income { get; set; } = 0;
        
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public List<Group> Groups { get; set; } = new List<Group>();

    }
}
