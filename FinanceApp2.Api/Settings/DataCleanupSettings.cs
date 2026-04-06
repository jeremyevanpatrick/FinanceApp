namespace FinanceApp2.Api.Settings
{
    public class DataCleanupSettings
    {
        public int PurgeSoftDeletedAfterDays { get; set; }
        public int PurgeTokensAfterDays { get; set; }
        public int PurgeInfoLogsAfterDays { get; set; }
        public int PurgeErrorLogsAfterDays { get; set; }
        public int ScheduledHour { get; set; }
    }
}
