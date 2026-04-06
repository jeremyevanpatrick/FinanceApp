namespace FinanceApp2.Shared.Helpers
{
    public sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        private NullScope() { }

        public void Dispose(){ }
    }
}
