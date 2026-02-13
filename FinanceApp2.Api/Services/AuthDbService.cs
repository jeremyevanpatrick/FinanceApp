using FinanceApp2.Api.Data;
using FinanceApp2.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Services
{
    public class AuthDbService : IAuthDbService
    {
        private readonly AuthDbContext _db;

        public AuthDbService(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<List<ApplicationUser>> GetSoftDeletedUsers(int olderThanDays = 0)
        {
            return await _db.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted && u.DeletedAt < DateTime.UtcNow.AddDays(-olderThanDays))
                .ToListAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash)
        {
            return await _db.RefreshTokens
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == refreshTokenHash &&
                    !t.IsRevoked &&
                    t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string refreshTokenHash)
        {
            var storedToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(string userId)
        {
            var userTokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteExpiredRefreshTokensAsync(int olderThanDays = 0)
        {
            await _db.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-olderThanDays))
                .ExecuteDeleteAsync();
        }
    }
}
