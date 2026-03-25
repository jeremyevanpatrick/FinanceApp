using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Models;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp2.Api.Tests.Repositories
{
    public class AuthRepositoryTests
    {
        private AuthDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AuthDbContext(options);
        }

        private AuthDbContext CreateSqLiteDb()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AuthDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetSoftDeletedUsersAsync_WhenDeletedUserExists_ReturnsUsers()
        {
            // Arrange
            var db = CreateDb();

            var deletedUserId = Guid.NewGuid().ToString();

            db.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                IsDeleted = false
            });
            db.Users.Add(new ApplicationUser
            {
                Id = deletedUserId,
                DeletedAt = DateTime.UtcNow.AddDays(-10),
                IsDeleted = true
            });
            db.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            });

            await db.SaveChangesAsync();

            var repo = new AuthRepository(db);

            // Act
            var result = await repo.GetSoftDeletedUsersAsync(1);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(u => u.Id == deletedUserId);
        }

        [Fact]
        public async Task GetUserRefreshTokensAsync_WhenUserRefreshTokensExist_ReturnsUserRefreshTokens()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid().ToString();

            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "Hash1",
                UserId = userId,
                IsRevoked = true
            });
            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "Hash2",
                UserId = userId,
                IsRevoked = false
            });
            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "Hash3",
                UserId = Guid.NewGuid().ToString(),
                IsRevoked = true
            });

            await db.SaveChangesAsync();

            var repo = new AuthRepository(db);

            // Act
            var result = await repo.GetUserRefreshTokensAsync(userId);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(r => r.TokenHash == "Hash2");
        }

        [Fact]
        public async Task GetRefreshTokenAsync_WhenRefreshTokenExists_ReturnsRefreshToken()
        {
            // Arrange
            var db = CreateDb();

            var testHash = "Hash2";

            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "Hash1",
                UserId = Guid.NewGuid().ToString()
            });
            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = testHash,
                UserId = Guid.NewGuid().ToString()
            });
            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = "Hash3",
                UserId = Guid.NewGuid().ToString()
            });

            await db.SaveChangesAsync();

            var repo = new AuthRepository(db);

            // Act
            var result = await repo.GetRefreshTokenAsync(testHash);

            // Assert
            result.Should().NotBeNull();
            result.TokenHash.Should().Be(testHash);
        }

        [Fact]
        public async Task AddRefreshTokenAsync_WhenRefreshTokenIsUnique_CreatesRefreshToken()
        {
            // Arrange
            var db = CreateDb();

            var testHash = "Hash1";

            var tokenToAdd = new RefreshToken
            {
                TokenHash = testHash,
                UserId = Guid.NewGuid().ToString()
            };
            
            var repo = new AuthRepository(db);

            // Act
            await repo.AddRefreshTokenAsync(tokenToAdd);

            // Assert
            var addedToken = await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync();
            addedToken!.Should().NotBeNull();
            addedToken.TokenHash.Should().Be(testHash);
        }

        [Fact]
        public async Task AddRefreshTokenAsync_WhenRefreshTokenIsDuplicate_ThrowsException()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var testHash = "Hash1";
            var userId = Guid.NewGuid().ToString();

            db.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = testHash,
                UserId = userId
            });

            await db.SaveChangesAsync();

            var tokenToAdd = new RefreshToken
            {
                TokenHash = testHash,
                UserId = userId
            };

            var repo = new AuthRepository(db);

            // Act
            Func<Task> result = () => repo.AddRefreshTokenAsync(tokenToAdd);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateRefreshTokenAsync_WhenRefreshTokenExists_UpdatesRefreshToken()
        {
            // Arrange
            var db = CreateDb();

            var token = new RefreshToken
            {
                Id = 1,
                TokenHash = "Hash1",
                UserId = Guid.NewGuid().ToString(),
                IsRevoked = false
            };

            db.RefreshTokens.Add(token);

            await db.SaveChangesAsync();

            token!.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            var repo = new AuthRepository(db);

            // Act
            await repo.UpdateRefreshTokenAsync(token);

            // Assert
            var updatedToken = await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync();
            updatedToken!.Should().NotBeNull();
            updatedToken.IsRevoked.Should().Be(true);
        }

        [Fact]
        public async Task UpdateRefreshTokenRangeAsync_WhenRefreshTokensAreValid_UpdatesRefreshTokens()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid().ToString();

            var token2 = new RefreshToken
            {
                Id = 2,
                TokenHash = "Hash2",
                UserId = userId,
                IsRevoked = false
            };

            var tokenList = new List<RefreshToken> {
                new RefreshToken {
                    Id = 1,
                    TokenHash = "Hash1",
                    UserId = userId,
                    IsRevoked = false
                },
                token2,
                new RefreshToken {
                    Id = 3,
                    TokenHash = "Hash3",
                    UserId = Guid.NewGuid().ToString(),
                    IsRevoked = false
                }
            };

            db.RefreshTokens.AddRange(tokenList);

            await db.SaveChangesAsync();

            token2.IsRevoked = true;
            token2.RevokedAt = DateTime.UtcNow;

            var tokenListToUpdate = new List<RefreshToken> {
                token2
            };

            var repo = new AuthRepository(db);

            // Act
            await repo.UpdateRefreshTokenRangeAsync(tokenListToUpdate);

            // Assert
            var updatedTokenList = await db.RefreshTokens
                .AsNoTracking()
                .ToListAsync();
            updatedTokenList!.Count.Should().Be(3);
            updatedTokenList.Where(t => t.IsRevoked).Count().Should().Be(1);
        }

        [Fact]
        public async Task UpdateRefreshTokenRangeAsync_WhenRefreshTokensNotFound_ThrowsException()
        {
            // Arrange
            var db = CreateDb();

            var userId = Guid.NewGuid().ToString();

            var token2 = new RefreshToken
            {
                Id = 2,
                TokenHash = "Hash2",
                UserId = userId,
                IsRevoked = false
            };

            var tokenList = new List<RefreshToken> {
                new RefreshToken {
                    Id = 1,
                    TokenHash = "Hash1",
                    UserId = userId,
                    IsRevoked = false
                },
                token2,
                new RefreshToken {
                    Id = 3,
                    TokenHash = "Hash3",
                    UserId = Guid.NewGuid().ToString(),
                    IsRevoked = false
                }
            };

            db.RefreshTokens.AddRange(tokenList);

            await db.SaveChangesAsync();

            token2.IsRevoked = true;
            token2.RevokedAt = DateTime.UtcNow;

            var tokenListToUpdate = new List<RefreshToken> {
                token2,
                new RefreshToken {
                    Id = 4,
                    TokenHash = "Hash4",
                    UserId = userId,
                    IsRevoked = true
                }
            };

            var repo = new AuthRepository(db);

            // Act
            Func<Task> result = () => repo.UpdateRefreshTokenRangeAsync(tokenListToUpdate);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task DeleteRefreshTokensExpiredByDaysAsync_WhenRefreshTokensAreValid_DeletesRefreshTokens()
        {
            // Arrange
            var db = CreateSqLiteDb();

            var userId = Guid.NewGuid().ToString();

            var tokenList = new List<RefreshToken> {
                new RefreshToken {
                    Id = 1,
                    TokenHash = "Hash1",
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                },
                new RefreshToken
                {
                    Id = 2,
                    TokenHash = "Hash2",
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(-10)
                },
                new RefreshToken {
                    Id = 3,
                    TokenHash = "Hash3",
                    UserId = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow
                }
            };

            db.RefreshTokens.AddRange(tokenList);

            await db.SaveChangesAsync();

            var repo = new AuthRepository(db);

            // Act
            await repo.DeleteRefreshTokensExpiredByDaysAsync(1);

            // Assert
            var updatedTokenList = await db.RefreshTokens
                .AsNoTracking()
                .ToListAsync();
            updatedTokenList!.Count.Should().Be(2);
            updatedTokenList.Should().NotContain(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-1));
        }

    }
}
