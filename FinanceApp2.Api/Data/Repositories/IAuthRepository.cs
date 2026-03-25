using FinanceApp2.Api.Models;

namespace FinanceApp2.Api.Data.Repositories
{
    public interface IAuthRepository
    {
        Task<List<ApplicationUser>> GetSoftDeletedUsersAsync(int olderThanDays = 0);
        Task<List<RefreshToken>> GetUserRefreshTokensAsync(string userId);
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenRangeAsync(List<RefreshToken> refreshTokenList);
        Task DeleteRefreshTokensExpiredByDaysAsync(int olderThanDays = 0);
    }
}
