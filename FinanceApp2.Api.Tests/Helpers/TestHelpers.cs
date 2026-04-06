using FinanceApp2.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace FinanceApp2.Api.Tests.Helpers
{
    public static class TestHelpers
    {
        public static void HasValidLinks(List<Link> links)
        {
            links.Should().Contain(l =>
                !string.IsNullOrEmpty(l.Method) &&
                !string.IsNullOrEmpty(l.Href) &&
                !string.IsNullOrEmpty(l.Rel));
        }

        public static Mock<LinkGenerator> CreateLinkGeneratorMock()
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

            return mockLinkGenerator;
        }

        private static string? GetCookieString(ControllerBase controller)
        {
            var headers = controller.ControllerContext.HttpContext.Response.Headers;
            headers.Should().ContainKey("Set-Cookie");
            return headers["Set-Cookie"].ToString();
        } 

        public static void HasCookie(ControllerBase controller, string cookieKeyValue)
        {
            GetCookieString(controller).Should().Contain(cookieKeyValue);
        }

        public static void NotHaveCookie(ControllerBase controller, string cookieKeyValue)
        {
            GetCookieString(controller).Should().NotContain(cookieKeyValue);
        }
    }
}
