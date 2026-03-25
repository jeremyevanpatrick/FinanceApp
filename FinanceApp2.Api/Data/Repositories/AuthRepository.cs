using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _db;

        public AuthRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<List<ApplicationUser>> GetSoftDeletedUsersAsync(int olderThanDays = 0)
        {
            return await _db.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted && u.DeletedAt < DateTime.UtcNow.AddDays(-olderThanDays))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RefreshToken>> GetUserRefreshTokensAsync(string userId)
        {
            return await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash)
        {
            return await _db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _db.RefreshTokens.Update(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenRangeAsync(List<RefreshToken> refreshTokenList)
        {
            _db.RefreshTokens.UpdateRange(refreshTokenList);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRefreshTokensExpiredByDaysAsync(int olderThanDays = 0)
        {
            await _db.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-olderThanDays))
                .ExecuteDeleteAsync();
        }
    }
}
