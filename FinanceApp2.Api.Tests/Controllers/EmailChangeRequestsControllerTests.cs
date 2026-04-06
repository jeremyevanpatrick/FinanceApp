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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services.Responses;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class EmailChangeRequestsControllerTests
    {
        private static EmailChangeRequestsController CreateController(Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);
            
            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockUsersLinkHelper = new UsersLinkHelper(mockLinkGenerator2.Object);

            var mockLinkGenerator3 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailChangeRequestsLinkHelper = new EmailChangeRequestsLinkHelper(mockLinkGenerator3.Object);

            var controller = new EmailChangeRequestsController(
                Mock.Of<ILogger<EmailChangeRequestsController>>(),
                mockSessionsLinkHelper,
                mockUsersLinkHelper,
                mockEmailChangeRequestsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static EmailChangeRequestsController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);

            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockUsersLinkHelper = new UsersLinkHelper(mockLinkGenerator2.Object);

            var mockLinkGenerator3 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailChangeRequestsLinkHelper = new EmailChangeRequestsLinkHelper(mockLinkGenerator3.Object);

            var controller = new EmailChangeRequestsController(
                Mock.Of<ILogger<EmailChangeRequestsController>>(),
                mockSessionsLinkHelper,
                mockUsersLinkHelper,
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
        public async Task ChangeEmail_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

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
        public async Task ChangeEmail_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

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

            errorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task ChangeEmail_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangeEmailConfirmation_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

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
        public async Task ChangeEmailConfirmation_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

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

            errorCode.Should().Be(ApiErrorCodes.AUTH_NO_LONGER_VALID.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task ChangeEmailConfirmation_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
