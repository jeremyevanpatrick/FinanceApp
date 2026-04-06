using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Tests.Helpers;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Services.Responses;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class PasswordResetRequestsControllerTests
    {
        private static PasswordResetRequestsController CreateController(Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);

            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockPasswordResetRequestsLinkHelper = new PasswordResetRequestsLinkHelper(mockLinkGenerator2.Object);

            var controller = new PasswordResetRequestsController(
                Mock.Of<ILogger<PasswordResetRequestsController>>(),
                mockSessionsLinkHelper,
                mockPasswordResetRequestsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact]
        public async Task ForgotPassword_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ForgotPasswordRequest { Email = "test" };
            var result = await controller.ForgotPassword(request);

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
        public async Task ForgotPassword_InternalError_ReturnsOkObjectResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ForgotPasswordRequest { Email = "test" };
            var result = await controller.ForgotPassword(request);

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
        public async Task ResetPassword_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

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
        public async Task ResetPassword_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

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

            errorCode.Should().Be(ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED.ToString());

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
        }

        [Fact]
        public async Task ResetPassword_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
