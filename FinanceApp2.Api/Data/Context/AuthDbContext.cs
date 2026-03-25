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
        modelBuilder.Entity<ApplicationUser>()
            .HasQueryFilter(u => !u.IsDeleted);

        // Refresh token configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(t => t.TokenHash).IsUnique();

            entity.HasIndex(t => new { t.UserId, t.IsRevoked });

            entity.HasIndex(t => t.ExpiresAt);

            entity.Property(t => t.TokenHash).IsRequired();
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.ExpiresAt).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
        });
    }

}