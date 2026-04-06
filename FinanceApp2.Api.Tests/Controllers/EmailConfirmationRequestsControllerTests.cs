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

namespace FinanceApp2.Api.Tests.Controllers
{
    public class EmailConfirmationRequestsControllerTests
    {
        private static EmailConfirmationRequestsController CreateController(Mock<IAuthAppService> mockService)
        {
            var mockLinkGenerator1 = TestHelpers.CreateLinkGeneratorMock();
            var mockSessionsLinkHelper = new SessionsLinkHelper(mockLinkGenerator1.Object);

            var mockLinkGenerator2 = TestHelpers.CreateLinkGeneratorMock();
            var mockEmailConfirmationRequestsLinkHelper = new EmailConfirmationRequestsLinkHelper(mockLinkGenerator2.Object);

            var controller = new EmailConfirmationRequestsController(
                Mock.Of<ILogger<EmailConfirmationRequestsController>>(),
                mockSessionsLinkHelper,
                mockEmailConfirmationRequestsLinkHelper,
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact]
        public async Task ResendConfirmationEmail_ValidRequest_ReturnsOkObjectResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResendConfirmationEmailRequest { Email = "test" };
            var result = await controller.ResendConfirmationEmail(request);

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
        public async Task ResendConfirmationEmail_InternalError_ReturnsOkObjectResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResendConfirmationEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ResendConfirmationEmailRequest { Email = "test" };
            var result = await controller.ResendConfirmationEmail(request);

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
        public async Task ConfirmEmail_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

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
        public async Task ConfirmEmail_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

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
        public async Task ConfirmEmail_InternalError_Returns500Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

            //Assert
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
