using FinanceApp2.Api.Data.Context;
using FinanceApp2.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Tests.Helpers
{
    public class LoggingDbContextTest : LoggingDbContext
    {
        public LoggingDbContextTest(DbContextOptions<LoggingDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base method to keep the original production configuration
            base.OnModelCreating(modelBuilder);

            // Override column types for SQLite
            modelBuilder.Entity<ApplicationLog>(entity =>
            {
                entity.Property(l => l.Message)
                    .HasColumnType("TEXT");

                entity.Property(l => l.MessageTemplate)
                    .HasColumnType("TEXT");

                entity.Property(l => l.Exception)
                    .HasColumnType("TEXT");
            });
        }
    }
}
