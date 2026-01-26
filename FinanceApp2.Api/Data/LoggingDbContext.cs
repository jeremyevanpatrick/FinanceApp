using FinanceApp2.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data
{
    public class LoggingDbContext : DbContext
    {
        public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
            : base(options) { }

        public DbSet<Error> Errors => Set<Error>();
    }
}
