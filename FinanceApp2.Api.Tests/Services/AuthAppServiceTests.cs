using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceApp2.Api.Tests.Services
{
    public class AuthAppServiceTests
    {
        private Mock<UserManager<ApplicationUser>> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object
            );
        }

        private Mock<SignInManager<ApplicationUser>> CreateSignInManager(Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                Options.Create(new IdentityOptions()),
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object
            );
        }

        [Fact]
        public async Task RegisterAsync_WhenAccountIsUnique_CreatesAccount()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            ApplicationUser? createdUser = null;
            string? createdPassword = null;
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, pwd) => {
                    createdUser = u;
                    createdPassword = pwd;
                })
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("testToken");

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            var clientSettings = new ClientSettings
            {
                Host = "testHost"
            };

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Options.Create(clientSettings),
                Mock.Of<IOptions<JwtSettings>>(),
                mockEmailSender.Object);

            var email = "testEmail";
            var password = "testPwd";

            // Act
            await service.RegisterAsync(email, password);

            // Assert
            createdUser.Should().NotBeNull();
            createdUser.Email.Should().Be(email);
            createdPassword.Should().Be(password);
            mockEmailSender.Verify(
                s => s.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenAccountIsDuplicate_DoesNothing()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testError",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            var email = "testEmail";
            var password = "testPwd";

            // Act
            Func<Task> result = () => service.RegisterAsync(email, password);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockUserManager.Verify(
                s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenAccountPasswordIsInvalid_ThrowsException()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordTest",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            var email = "testEmail";
            var password = "testPwd";

            // Act
            Func<Task> result = () => service.RegisterAsync(email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task ResendConfirmationEmailAsync_WhenAccountIsUnique_SendsEmail()
        {
            // Arrange
            var email = "testEmail";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser()
                {
                    EmailConfirmed = false,
                    Email = email,
                    UserName = email
                });
            mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("testToken");

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var clientSettings = new ClientSettings
            {
                Host = "testHost"
            };

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Options.Create(clientSettings),
                Mock.Of<IOptions<JwtSettings>>(),
                mockEmailSender.Object);

            // Act
            Func<Task> result = () => service.ResendConfirmationEmailAsync(email);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockEmailSender.Verify(
                s => s.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ResendConfirmationEmailAsync_WhenEmailIsEmpty_DoesNothing()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            var email = "";

            // Act
            await service.ResendConfirmationEmailAsync(email);

            // Assert
            mockUserManager.Verify(
                s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task ResendConfirmationEmailAsync_WhenAccountNotFound_DoesNothing()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            var email = "testEmail";

            // Act
            await service.ResendConfirmationEmailAsync(email);

            // Assert
            mockUserManager.Verify(
                s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task ResendConfirmationEmailAsync_WhenEmailAlreadyConfirmed_DoesNothing()
        {
            // Arrange
            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { EmailConfirmed = true });

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            var email = "testEmail";

            // Act
            await service.ResendConfirmationEmailAsync(email);

            // Assert
            mockUserManager.Verify(
                s => s.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenValid_ConfirmsEmail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var token = "test";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = "testEmail", UserName = "testEmail" });
            mockUserManager.Setup(m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ConfirmEmailAsync(userId, token);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockUserManager.Verify(
                s => s.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var token = "test";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>()); 

            // Act
            Func<Task> result = () => service.ConfirmEmailAsync(userId, token);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenTokenIsExpired_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var token = "test";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = "testEmail", UserName = "testEmail" });
            mockUserManager.Setup(m => m.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testErrorCode",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ConfirmEmailAsync(userId, token);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task LoginAsync_WhenValid_ReturnsAuthResponse()
        {
            // Arrange
            var email = "testEmail";
            var password = "testPassword";
            var userId = Guid.NewGuid().ToString();

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });

            var mockSignInManager = CreateSignInManager(mockUserManager);
            mockSignInManager.Setup(m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            var jwtSettings = Options.Create(new JwtSettings
            {
                Audience = "testAudience",
                ExpireMinutes = 10,
                Issuer = "testIssuer",
                Key = "testKey_at_least_16_characters_long"
            });

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                jwtSettings,
                Mock.Of<IEmailSender>());

            // Act
            var result = await service.LoginAsync(email, password);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.AccessToken.Should().NotBeNull();
            result.RefreshToken.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LoginAsync(email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task LoginAsync_WhenUserIsLockedOut_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var password = "testPassword";
            var userId = Guid.NewGuid().ToString();

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.GetLockoutEndDateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new DateTimeOffset());

            var mockSignInManager = CreateSignInManager(mockUserManager);
            mockSignInManager.Setup(m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.LockedOut);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LoginAsync(email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsInvalid_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var password = "testPassword";
            var userId = Guid.NewGuid().ToString();

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });

            var mockSignInManager = CreateSignInManager(mockUserManager);
            mockSignInManager.Setup(m => m.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.NotAllowed);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LoginAsync(email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenEmailIsValid_SendsEmail()
        {
            // Arrange
            var email = "testEmail";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);
            mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("testToken");

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var clientSettings = Options.Create(new ClientSettings
            {
                Host = "testHost"
            });

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                clientSettings,
                Mock.Of<IOptions<JwtSettings>>(),
                mockEmailSender.Object);

            // Act
            Func<Task> result = () => service.ForgotPasswordAsync(email);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockEmailSender.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenUserNotFound_DoesNothing()
        {
            // Arrange
            var email = "testEmail";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ForgotPasswordAsync(email);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockUserManager.Verify(
                s => s.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenValid_ResetsPassword()
        {
            // Arrange
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ResetPasswordAsync(email, token, password);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

        }

        [Fact]
        public async Task ResetPasswordAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            
            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ResetPasswordAsync(email, token, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenTokenIsExpired_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testErrorCode",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ResetPasswordAsync(email, token, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenValid_ChangesPassword()
        {
            // Arrange
            var email = "testEmail";
            var oldPassword = "testPassword";
            var newPassword = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangePasswordAsync(email, oldPassword, newPassword);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

        }

        [Fact]
        public async Task ChangePasswordAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var oldPassword = "testPassword";
            var newPassword = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangePasswordAsync(email, oldPassword, newPassword);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenPasswordIsWrong_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var oldPassword = "testPassword";
            var newPassword = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordMismatch",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangePasswordAsync(email, oldPassword, newPassword);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenFailed_ThrowsException()
        {
            // Arrange
            var email = "testEmail";
            var oldPassword = "testPassword";
            var newPassword = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = Guid.NewGuid().ToString(), Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testErrorCode",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangePasswordAsync(email, oldPassword, newPassword);

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ChangeEmailAsync_WhenValid_ChangesEmail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            mockUserManager.Setup(m => m.GenerateChangeEmailTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync("testKey_at_least_16_characters_long");

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var clientSettings = Options.Create(new ClientSettings
            {
                Host = "testHost"
            });

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                clientSettings,
                Mock.Of<IOptions<JwtSettings>>(),
                mockEmailSender.Object);

            // Act
            Func<Task> result = () => service.ChangeEmailAsync(userId, email, password);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockEmailSender.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangeEmailAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailAsync(userId, email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangeEmailAsync_WhenPasswordIsWrong_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailAsync(userId, email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangeEmailAsync_WhenEmailAlreadyClaimed_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var password = "testPassword";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailAsync(userId, email, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task ChangeEmailConfirmationAsync_WhenValid_ChangesEmail()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var clientSettings = Options.Create(new ClientSettings
            {
                Host = "testHost"
            });

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                clientSettings,
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailConfirmationAsync(userId, email, token);

            // Assert
            await result.Should().NotThrowAsync<Exception>();
            mockUserManager.Verify(
                s => s.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangeEmailConfirmationAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailConfirmationAsync(userId, email, token);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangeEmailConfirmationAsync_WhenPasswordIsWrong_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var token = "testKey_at_least_16_characters_long";

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testErrorCode",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.ChangeEmailConfirmationAsync(userId, email, token);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenValid_ReturnsAuthResponse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "testEmail";
            var refreshToken = "testKey_at_least_16_characters_long";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken() { Id = 123, UserId = Guid.NewGuid().ToString(), TokenHash = "testHash", IsRevoked = false, ExpiresAt = DateTime.UtcNow.AddDays(7) });
            mockRepo.Setup(m => m.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(m => m.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser() { Id = userId, Email = email, UserName = email });
            mockUserManager.Setup(m => m.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var jwtSettings = Options.Create(new JwtSettings
            {
                Audience = "testAudience",
                ExpireMinutes = 10,
                Issuer = "testIssuer",
                Key = "testKey_at_least_16_characters_long"
            });

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                jwtSettings,
                Mock.Of<IEmailSender>());

            // Act
            var result = await service.RefreshTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNull();
            result.RefreshToken.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIsEmpty_ThrowsException()
        {
            // Arrange
            var refreshToken = string.Empty;

            var mockUserManager = CreateUserManager();
            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.RefreshTokenAsync(refreshToken);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIsExpired_ThrowsException()
        {
            // Arrange
            var refreshToken = "testKey_at_least_16_characters_long";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken()
                {
                    Id = 123,
                    UserId = Guid.NewGuid().ToString(),
                    TokenHash = "testHash",
                    IsRevoked = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(-1)
                });

            var mockUserManager = CreateUserManager();

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.RefreshTokenAsync(refreshToken);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
        }

        [Fact]
        public async Task LogoutAsync_WhenValid_RevokesToken()
        {
            // Arrange
            var refreshToken = "testKey_at_least_16_characters_long";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken()
                {
                    Id = 123,
                    UserId = Guid.NewGuid().ToString(),
                    TokenHash = "testHash",
                    IsRevoked = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                });
            RefreshToken? updatedToken = null;
            mockRepo.Setup(m => m.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => updatedToken = t)
                .Returns(Task.CompletedTask);

            var mockUserManager = CreateUserManager();

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LogoutAsync(refreshToken);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            updatedToken.Should().NotBeNull();
            updatedToken.IsRevoked.Should().Be(true);
        }

        [Fact]
        public async Task LogoutAsync_WhenTokenIsEmpty_DoesNothing()
        {
            // Arrange
            var refreshToken = string.Empty;

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshToken?)null);

            var mockUserManager = CreateUserManager();

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                Mock.Of<IAuthRepository>(),
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LogoutAsync(refreshToken);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            mockRepo.Verify(
                s => s.GetRefreshTokenAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WhenTokenIsExpired_DoesNothing()
        {
            // Arrange
            var refreshToken = "testKey_at_least_16_characters_long";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshToken()
                {
                    Id = 123,
                    UserId = Guid.NewGuid().ToString(),
                    TokenHash = "testHash",
                    IsRevoked = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(-1)
                });
            mockRepo.Setup(m => m.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            var mockUserManager = CreateUserManager();

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.LogoutAsync(refreshToken);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            mockRepo.Verify(
                s => s.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAccountAsync_WhenValid_SoftDeletesUser()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var password = "testPassword";
            var email = "testEmail";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetUserRefreshTokensAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<RefreshToken>()
                {
                    new RefreshToken()
                    {
                        Id = 111,
                        UserId = userId,
                        TokenHash = "testHashActive",
                        IsRevoked = false,
                        ExpiresAt = DateTime.UtcNow.AddDays(1)
                    },
                    new RefreshToken()
                    {
                        Id = 112,
                        UserId = userId,
                        TokenHash = "testHashExpired",
                        IsRevoked = false,
                        ExpiresAt = DateTime.UtcNow.AddDays(-1)
                    }
                });
            List<RefreshToken> updatedTokens = new List<RefreshToken>();
            mockRepo.Setup(m => m.UpdateRefreshTokenRangeAsync(It.IsAny<List<RefreshToken>>()))
                .Callback<List<RefreshToken>>(t => updatedTokens.AddRange(t))
                .Returns(Task.CompletedTask);

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser()
                {
                    Id = userId,
                    Email = email,
                    UserName = email,
                    IsDeleted = false
                });
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            ApplicationUser? user = null;
            mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .Callback<ApplicationUser>(u => user = u)
                .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.DeleteAccountAsync(userId, password);

            // Assert
            await result.Should().NotThrowAsync<AuthException>();
            user.Should().NotBeNull();
            user.IsDeleted.Should().Be(true);
            updatedTokens.Should().NotBeNull();
            updatedTokens.Count().Should().Be(2);
            updatedTokens.Should().NotContain(x => x.IsRevoked == false);
        }

        [Fact]
        public async Task DeleteAccountAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var password = "testPassword";

            var mockRepo = new Mock<IAuthRepository>();

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.DeleteAccountAsync(userId, password);

            // Assert
            await result.Should().ThrowAsync<AuthException>();
            mockUserManager.Verify(
                s => s.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAccountAsync_WhenPasswordIsInvalid_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var password = "testPassword";
            var email = "testEmail";

            var mockRepo = new Mock<IAuthRepository>();
            mockRepo.Setup(m => m.GetUserRefreshTokensAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<RefreshToken>());

            var mockUserManager = CreateUserManager();
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser()
                {
                    Id = userId,
                    Email = email,
                    UserName = email,
                    IsDeleted = false
                });
            mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "testErrorCode",
                    Description = "testError"
                }));

            var mockSignInManager = CreateSignInManager(mockUserManager);

            var service = new AuthAppService(
                mockRepo.Object,
                mockUserManager.Object,
                mockSignInManager.Object,
                Mock.Of<IOptions<ClientSettings>>(),
                Mock.Of<IOptions<JwtSettings>>(),
                Mock.Of<IEmailSender>());

            // Act
            Func<Task> result = () => service.DeleteAccountAsync(userId, password);

            // Assert
            await result.Should().ThrowAsync<Exception>();
            mockRepo.Verify(
                s => s.GetUserRefreshTokensAsync(It.IsAny<string>()),
                Times.Never);
        }

    }
}