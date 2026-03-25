using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class EmailConfirmationsControllerTests
    {
        private static EmailConfirmationsController CreateController(Mock<IAuthAppService> mockService)
        {
            var controller = new EmailConfirmationsController(
                Mock.Of<ILogger<EmailConfirmationsController>>(),
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact]
        public async Task ResendConfirmationEmail_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResendConfirmationEmailRequest { Email = "test" };
            var result = await controller.ResendConfirmationEmail(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ResendConfirmationEmail_InternalError_ReturnsNoContentResult()
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
            result.Should().BeOfType<NoContentResult>();
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
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ConfirmEmail_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ConfirmEmailRequest(Guid.NewGuid().ToString(), "test");
            var result = await controller.ConfirmEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangeEmailConfirmation_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ChangeEmailConfirmation_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateController(mockService);

            //Act
            var request = new ChangeEmailConfirmationRequest(userId.ToString(), "test", "test");
            var result = await controller.ChangeEmailConfirmation(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.AUTH_NO_LONGER_VALID.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
