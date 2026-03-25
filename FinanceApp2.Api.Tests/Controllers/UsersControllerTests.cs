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
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class UsersControllerTests
    {
        private static UsersController CreateController(Mock<IAuthAppService> mockService)
        {
            var controller = new UsersController(
                Mock.Of<ILogger<UsersController>>(),
                mockService.Object);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        private static UsersController CreateControllerWithUser(Guid userId, Mock<IAuthAppService> mockService)
        {
            var controller = new UsersController(
                Mock.Of<ILogger<UsersController>>(),
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
            result.Should().BeOfType<CreatedResult>();
        }

        [Fact]
        public async Task Register_NotFound_Returns400Result()
        {
            //Arrange
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS));

            var controller = CreateController(mockService);

            //Act
            var request = new RegisterRequest { Email = "test", Password = "test" };
            var result = await controller.Register(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangePassword_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ChangePassword_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangePasswordRequest("test", "test");
            var result = await controller.ChangePassword(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.AUTH_NO_LONGER_VALID.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ChangeEmail_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ChangeEmail_NotFound_Returns400Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

            mockService
                .Setup(s => s.ChangeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new ChangeEmailRequest("test", "test");
            var result = await controller.ChangeEmail(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.AUTH_NO_LONGER_VALID.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteAccount_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IAuthAppService>();

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
                .ThrowsAsync(new AuthException("Test error", StatusCodes.Status400BadRequest, ResponseErrorCodes.AUTH_NO_LONGER_VALID));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var request = new DeleteAccountRequest("test");
            var result = await controller.DeleteAccount(request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(400);
            result.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeOfType<ProblemDetails>()
                .Which.Extensions["errorCode"].Should().Be(ResponseErrorCodes.AUTH_NO_LONGER_VALID.ToString());
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
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

    }
}
