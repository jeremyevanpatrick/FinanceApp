using FinanceApp2.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data.Context
{
    public class LoggingDbContext : DbContext
    {
        public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
            : base(options) { }

        public DbSet<ApplicationLog> ApplicationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationLog>(entity =>
            {
                entity.HasKey(l => l.Id);
                
                entity.Property(l => l.Level)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(l => l.ErrorCode)
                    .HasMaxLength(32);

                entity.Property(l => l.Message)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(l => l.MessageTemplate)
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(l => l.Exception)
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(l => l.CorrelationId)
                    .HasMaxLength(64);

                entity.Property(l => l.ServerName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(l => l.ApplicationName)
                    .IsRequired()
                    .HasMaxLength(128);

            });
        }
    }
}
