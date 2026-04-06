using FinanceApp2.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data.Context;

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

            entity.Property(e => e.Month)
                .IsRequired();

            entity.Property(e => e.Year)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.ModifiedAt)
                .IsRequired(false);

            entity.Property(e => e.IsDeleted)
                .IsRequired();

            entity.HasIndex(e => new { e.UserId, e.Year, e.Month })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasMany(e => e.Groups)
                .WithOne(e => e.Budget)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Group configuration
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.BudgetId)
                .IsRequired();

            entity.Property(e => e.GroupName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Order)
                .IsRequired();

            entity.Property(e => e.ModifiedAt)
                .IsRequired(false);

            entity.Property(e => e.IsDeleted)
                .IsRequired();

            entity.HasIndex(e => e.BudgetId);

            entity.HasOne(g => g.Budget)
                .WithMany(b => b.Groups)
                .HasForeignKey(g => g.BudgetId);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.Group)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Item configuration
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId);

            entity.Property(e => e.GroupId)
                .IsRequired();

            entity.Property(e => e.ItemName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Spent)
                .IsRequired(false);

            entity.Property(e => e.Budgeted)
                .IsRequired(false);

            entity.Property(e => e.ModifiedAt)
                .IsRequired(false);

            entity.Property(e => e.IsDeleted)
                .IsRequired();

            entity.HasOne(i => i.Group)
                .WithMany(g => g.Items)
                .HasForeignKey(i => i.GroupId);
        });
    }
}