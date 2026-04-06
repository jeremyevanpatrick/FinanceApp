using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Tests.Helpers;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services.Responses;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class UsersControllerTests
    {
        private static UsersController CreateController(Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);

            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockUsersLinkHelper = new UsersLinkHelper(mockLinkGenerator2.Object);

            var mockLinkGenerator3 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailConfirmationRequestsLinkHelper = new EmailConfirmationRequestsLinkHelper(mockLinkGenerator3.Object);

            var mockLinkGenerator4 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailChangeRequestsLinkHelper = new EmailChangeRequestsLinkHelper(mockLinkGenerator4.Object);

            var controller = new UsersController(
                Mock.Of<ILogger<UsersController>>(),
                mockSessionsLinkHelper,
                mockUsersLinkHelper,
                mockEmailConfirmationRequestsLinkHelper,
                mockEmailChangeRequestsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static UsersController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);

            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockUsersLinkHelper = new UsersLinkHelper(mockLinkGenerator2.Object);

            var mockLinkGenerator3 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailConfirmationRequestsLinkHelper = new EmailConfirmationRequestsLinkHelper(mockLinkGenerator3.Object);

            var mockLinkGenerator4 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailChangeRequestsLinkHelper = new EmailChangeRequestsLinkHelper(mockLinkGenerator4.Object);

            var controller = new UsersController(
                Mock.Of<ILogger<UsersController>>(),
                mockSessionsLinkHelper,
                mockUsersLinkHelper,
                mockEmailConfirmationRequestsLinkHelper,
                mockEmailChangeRequestsLinkHelper,
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
        public async Task Register_ValidRequest_ReturnsCreatedResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<CreatedResult>()
                .Subject;

            var links = httpResult
                .Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task Register_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS));

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

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

            apiErrorResponse.Status.Should().Be(400);

            var errorCode = apiErrorResponse.ErrorCode
                .Should()
                .BeAssignableTo<string>()
                .Subject;

            errorCode.Should().Be(ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task Register_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangePassword_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var links = httpResult
                .Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task ChangePassword_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status401Unauthorized, ApiErrorCodes.INVALID_CREDENTIALS));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

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
        public async Task ChangePassword_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteAccount_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();
            mockService
                .Setup(s => s.DeleteAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAccount_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.DeleteAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            var apiErrorResponse = result!
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(400);

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
        public async Task DeleteAccount_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.DeleteAccountAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result!.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
