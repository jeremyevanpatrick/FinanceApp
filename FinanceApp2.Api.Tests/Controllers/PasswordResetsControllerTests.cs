using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class PasswordResetsControllerTests
    {
        private static PasswordResetsController CreateController(Mock<IAuthAppService> mockService)
        {
            var controller = new PasswordResetsController(
                Mock.Of<ILogger<PasswordResetsController>>(),
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact]
        public async Task ForgotPassword_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ForgotPasswordRequest { Email = "test" };
            var result = await controller.ForgotPassword(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ForgotPassword_InternalError_ReturnsNoContentResult()
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
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ResetPassword_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ResetPassword_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED));

            var controller = CreateController(mockService);

            //Act
            var request = new ResetPasswordRequest { Email = "test", ResetCode = "test", NewPassword = "test" };
            var result = await controller.ResetPassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
