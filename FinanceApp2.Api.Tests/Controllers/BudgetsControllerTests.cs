using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Services.Application;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FinanceApp2.Api.Tests.Controllers
{
    public class BudgetsControllerTests
    {
        private static BudgetsController CreateControllerWithUser(Guid userId, Mock<IBudgetAppService> mockService)
        {
            var controller = new BudgetsController(
                Mock.Of<ILogger<BudgetsController>>(),
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
        public async Task Get_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var budgetContainer = new BudgetContainer
            {
                Budget = new BudgetDto
                {
                    BudgetId = budgetId,
                    UserId = userId,
                    Month = month,
                    Year = year,
                    Income = 5000,
                    Groups = new List<GroupDto>(),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ModifiedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.GetByDateAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(budgetContainer);

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Get(year, month);

            //Assert
            var resultObject = result!.Result.Should().BeOfType<OkObjectResult>().Subject;
            var resultBudgetContainer = (BudgetContainer)resultObject.Value;
            resultBudgetContainer.Budget.Should().NotBeNull();
            resultBudgetContainer.Budget.Month.Should().Be(month);
            resultBudgetContainer.Budget.Year.Should().Be(year);
        }

        [Fact]
        public async Task Get_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.GetByDateAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Get(year, month);

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsOkResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new CreateBudgetRequest()
            {
                NewBudgetMonth = month,
                NewBudgetYear = year,
                SourceBudgetMonth = month - 1,
                SourceBudgetYear = year - 1
            };

            var budget = new BudgetDto
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                Month = month,
                Year = year,
                Income = 5000,
                Groups = new List<GroupDto>(),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.CreateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(budget);

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Create(request);

            //Assert
            var createdResult = result!.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(controller.Get));
            createdResult!.RouteValues!["year"].Should().Be(year);
            createdResult.RouteValues["month"].Should().Be(month);
            var budgetObject = (BudgetDto)createdResult.Value;
            budgetObject.Month.Should().Be(month);
            budgetObject.Year.Should().Be(year);
        }

        [Fact]
        public async Task Create_ConflictError_Returns409Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new CreateBudgetRequest()
            {
                NewBudgetMonth = month,
                NewBudgetYear = year
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.CreateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ThrowsAsync(new BudgetConflictException("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Create(request);

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task Create_NotFoundError_Returns404Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new CreateBudgetRequest()
            {
                NewBudgetMonth = month,
                NewBudgetYear = year
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.CreateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ThrowsAsync(new NotFoundException("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Create(request);

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Create_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new CreateBudgetRequest()
            {
                NewBudgetMonth = month,
                NewBudgetYear = year,
                SourceBudgetMonth = month - 1,
                SourceBudgetYear = year - 1
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.CreateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Create(request);

            //Assert
            result.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new UpdateBudgetRequest()
            {
                Income = 100,
                Groups = new List<GroupDto>()
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.UpdateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<List<GroupDto>>()))
                .Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Update(year, month, request);

            //Assert
            result!.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_NotFoundError_Returns404Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new UpdateBudgetRequest()
            {
                Income = 100,
                Groups = new List<GroupDto>()
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.UpdateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<List<GroupDto>>()))
                .ThrowsAsync(new NotFoundException("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Update(year, month, request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Update_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var request = new UpdateBudgetRequest()
            {
                Income = 100,
                Groups = new List<GroupDto>()
            };

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.UpdateAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<List<GroupDto>>()))
                .ThrowsAsync(new Exception("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Update(year, month, request);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Delete_ValidRequest_ReturnsNoContentResult()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.DeleteAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Delete(year, month);

            //Assert
            result!.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_NotFoundError_Returns404Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.DeleteAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new NotFoundException("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Delete(year, month);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Delete_InternalError_Returns500Result()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var year = 2026;
            var month = 3;

            var mockService = new Mock<IBudgetAppService>();
            mockService
                .Setup(s => s.DeleteAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("test"));

            var controller = CreateControllerWithUser(userId, mockService);

            //Act
            var result = await controller.Delete(year, month);

            //Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
