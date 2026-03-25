using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class SessionsControllerTests
    {
        private static SessionsController CreateController(Mock<IAuthAppService> mockService)
        {
            var controller = new SessionsController(
                Mock.Of<ILogger<SessionsController>>(),
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static SessionsController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var controller = new SessionsController(
                Mock.Of<ILogger<SessionsController>>(),
                mockService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthResponse() { UserId = Guid.NewGuid().ToString() });
            mockService
                .Setup(s => s.CreateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync("testRefreshToken");

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<OkObjectResult>();
            var headers = controller.ControllerContext.HttpContext.Response.Headers;
            headers.Should().ContainKey("Set-Cookie");
            headers["Set-Cookie"].ToString()
                .Should().Contain("refresh_token=testRefreshToken");
        }

        [Fact]
        public async Task Login_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.INVALID_CREDENTIALS));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.INVALID_CREDENTIALS.ToString());
        }

        [Fact]
        public async Task Login_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Logout_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Logout();

            //Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Logout_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LogoutAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Logout();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new AuthResponse() { UserId = Guid.NewGuid().ToString() });
            mockService
                .Setup(s => s.CreateRefreshTokenAsync(It.IsAny<string>()))
                .ReturnsAsync("testRefreshToken");

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=oldRefreshToken";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<OkObjectResult>();
            var headers = controller.ControllerContext.HttpContext.Response.Headers;
            headers.Should().ContainKey("Set-Cookie");
            headers["Set-Cookie"].ToString()
                .Should().Contain("refresh_token=testRefreshToken");
        }

        [Fact]
        public async Task RefreshToken_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=test_invalid_token";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.AUTH_NO_LONGER_VALID.ToString());
        }

        [Fact]
        public async Task RefreshToken_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
