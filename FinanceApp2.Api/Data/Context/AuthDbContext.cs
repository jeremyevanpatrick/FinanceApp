using FinanceApp2.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data.Context;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Application user configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);

            entity.Property(u => u.IsDeleted)
                .IsRequired();

            entity.Property(u => u.DeletedAt)
                .IsRequired(false);
        });

        // Refresh token configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasIndex(t => t.TokenHash)
                .IsUnique();

            entity.HasIndex(t => t.UserId);

            entity.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(t => t.UserId)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            entity.Property(t => t.ExpiresAt)
                .IsRequired();

            entity.Property(t => t.IsRevoked)
                .IsRequired();

            entity.Property(t => t.RevokedAt)
                .IsRequired(false);

        });
    }

}