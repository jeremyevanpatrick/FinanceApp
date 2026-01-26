using FinanceApp2.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Budget configuration
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId);
            entity.Property(e => e.BudgetId).HasMaxLength(36);
            entity.Property(e => e.Month).HasMaxLength(2).IsRequired();
            entity.Property(e => e.Year).HasMaxLength(2).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Income).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => new { e.UserId, e.Year, e.Month }).IsUnique().HasFilter("[IsDeleted] = 0");

            entity.HasMany(e => e.Groups)
                .WithOne(e => e.Budget)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Group configuration
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId);
            entity.Property(e => e.GroupId).HasMaxLength(36);
            entity.Property(e => e.BudgetId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.GroupName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Order).HasMaxLength(4).IsRequired();

            entity.HasIndex(e => e.BudgetId);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.Group)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Item configuration
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).HasMaxLength(36);
            entity.Property(e => e.GroupId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.ItemName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Budgeted).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Spent).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.GroupId);
        });
    }
}