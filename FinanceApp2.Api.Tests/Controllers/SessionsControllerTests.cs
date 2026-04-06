using Azure.Core;
using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Tests.Helpers;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services.Responses;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class SessionsControllerTests
    {
        private static SessionsController CreateController(Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator = TestHelpers.CreateLinkGeneratorMock();

            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator.Object);

            var controller = new SessionsController(
                Mock.Of<ILogger<SessionsController>>(),
                mockSessionsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static SessionsController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator = new Mock<LinkGenerator>();
            mockLinkGenerator
                .Setup(lg => lg.GetPathByAddress(
                    It.IsAny<object>(),
                    It.IsAny<RouteValueDictionary>(),
                    It.IsAny<PathString>(),
                    It.IsAny<FragmentString>(),
                    It.IsAny<LinkOptions>()
                ))
                .Returns((object address, RouteValueDictionary routes, PathString path, FragmentString fragment, LinkOptions linkOptions) => {
                    var type = address.GetType();
                    var controller = type.GetProperty("controller")?.GetValue(address)?.ToString();
                    var action = type.GetProperty("action")?.GetValue(address)?.ToString();
                    var url = $"/{controller}/{action}";
                    return url;
                });

            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            var httpContext = new DefaultHttpContext() { User = user };

            var controller = new SessionsController(
                Mock.Of<ILogger<SessionsController>>(),
                mockSessionsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var cookieValue = "testRefreshToken";
            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((new AuthResponse() { AccessToken = "testAccessToken" }, cookieValue));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var resultAuthResponse = httpResult
                .Value
                .Should()
                .BeAssignableTo<AuthResponse>()
                .Subject;

            resultAuthResponse!.AccessToken!.Should().NotBeNull();

            TestHelpers.HasCookie(controller, $"refresh_token={cookieValue}");
            TestHelpers.HasValidLinks(resultAuthResponse.Links);
        }

        [Fact]
        public async Task Login_NotFound_Returns401Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS));

            var controller = CreateController(mockService);

            //Act
            var request = new LoginRequest { Email = "test", Password = "test" };
            var result = await controller.Login(request);

            //Assert
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(401);

            var errorCode = apiErrorResponse.ErrorCode
                .Should()
                .BeAssignableTo<string>()
                .Subject;

            errorCode.Should().Be(ApiErrorCodes.INVALID_CREDENTIALS.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Logout_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var cookieValue = "testRefreshToken";
            var userId = Guid.NewGuid();

            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser(userId, mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = $"refresh_token={cookieValue}";

            //Act
            var result = await controller.Logout();

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            TestHelpers.NotHaveCookie(controller, $"refresh_token={cookieValue}");

            var links = httpResult
                .Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task Logout_InvalidToken_ReturnsOkResult()
        {
            //Arrange
            var cookieValue = "testRefreshToken";
            var userId = Guid.NewGuid();

            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = $"refresh_token={cookieValue}";

            //Act
            var result = await controller.Logout();

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            TestHelpers.NotHaveCookie(controller, $"refresh_token={cookieValue}");

            var links = httpResult
                .Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task Logout_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Logout();

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var cookieValue = "testRefreshToken";
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string?>()))
                .ReturnsAsync((new AuthResponse() { UserId = Guid.NewGuid().ToString(), AccessToken = "testAccessToken" }, cookieValue));

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=oldRefreshToken";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var resultAuthResponse = httpResult
                .Value
                .Should()
                .BeAssignableTo<AuthResponse>()
                .Subject;

            resultAuthResponse!.AccessToken!.Should().NotBeNull();

            TestHelpers.HasCookie(controller, $"refresh_token={cookieValue}");
            TestHelpers.HasValidLinks(resultAuthResponse.Links);
        }

        [Fact]
        public async Task RefreshToken_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string?>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status401Unauthorized, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);
            controller.ControllerContext.HttpContext.Request.Headers["Cookies"] = "refresh_token=test_invalid_token";

            //Act
            var result = await controller.RefreshToken();

            //Assert
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(401);

            var errorCode = apiErrorResponse.ErrorCode
                .Should()
                .BeAssignableTo<string>()
                .Subject;

            errorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task RefreshToken_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RotateRefreshTokenAsync(It.IsAny<string?>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var result = await controller.RefreshToken();

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
