namespace FinanceApp2.Shared.Models
{
    public abstract class Resource
    {
        public List<Link> Links { get; set; } = new();
    }
}
