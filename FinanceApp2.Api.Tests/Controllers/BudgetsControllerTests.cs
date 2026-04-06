using FinanceApp2.Api.Controllers;
using FinanceApp2.Api.Exceptions;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Tests.Helpers;
using FinanceApp2.Shared.Models;
using FinanceApp2.Shared.Services.DTOs;
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
    public class BudgetsControllerTests
    {
        private static BudgetsController CreateControllerWithUser(Guid userId, Mock<IBudgetAppService> mockService)
        {
            var mockLinkGenerator = TestHelpers.CreateLinkGeneratorMock();

            var mockBudgetsLinkHelper = new BudgetsLinkHelper(mockLinkGenerator.Object);

            var controller = new BudgetsController(
                Mock.Of<ILogger<BudgetsController>>(),
                mockBudgetsLinkHelper,
                mockService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext()
                {
                    User = user,
                    Items = new Dictionary<object, object?> {
                        { "CorrelationId", Guid.NewGuid().ToString() }
                    }
                }
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

            var budgetContainer = new BudgetContainerDto
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
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var resultBudgetContainer = httpResult
                .Value
                .Should()
                .BeAssignableTo<BudgetContainerDto>()
                .Subject;

            resultBudgetContainer!.Budget!.Month.Should().Be(month);
            resultBudgetContainer.Budget.Year.Should().Be(year);

            TestHelpers.HasValidLinks(resultBudgetContainer.Links);
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
            result!.Result.Should().BeOfType<ObjectResult>()
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
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<CreatedAtActionResult>()
                .Subject;

            httpResult!.ActionName.Should().Be(nameof(controller.Get));
            httpResult.RouteValues!["year"].Should().Be(year);
            httpResult.RouteValues["month"].Should().Be(month);

            var resultBudget = httpResult
                .Value
                .Should()
                .BeAssignableTo<BudgetDto>()
                .Subject;

            resultBudget!.Month.Should().Be(month);
            resultBudget.Year.Should().Be(year);

            TestHelpers.HasValidLinks(resultBudget.Links);
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
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(409);

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(404);

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            result!.Result.Should().BeOfType<ObjectResult>()
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
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var links = httpResult.Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(404);

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Delete_ValidRequest_ReturnsOkObjectResult()
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
            var httpResult = result!
                .Result
                .Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var links = httpResult.Value
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            var apiErrorResponse = result!
                .Result
                .Should()
                .BeOfType<ObjectResult>()
                .Subject
                .Value
                .Should()
                .BeAssignableTo<ApiErrorResponse>()
                .Subject;

            apiErrorResponse.Status.Should().Be(404);

            var links = apiErrorResponse.Links
                .Should()
                .BeAssignableTo<List<Link>>()
                .Subject;

            TestHelpers.HasValidLinks(links);
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
            result!.Result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }
    }
}
