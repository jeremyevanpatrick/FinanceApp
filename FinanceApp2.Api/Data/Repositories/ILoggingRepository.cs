namespace FinanceApp2.Api.Data.Repositories
{
    public interface ILoggingRepository
    {
        Task DeleteInfoLogsByDaysAsync(int purgeAfterDays);
        Task DeleteErrorLogsByDaysAsync(int purgeAfterDays);
    }
}
