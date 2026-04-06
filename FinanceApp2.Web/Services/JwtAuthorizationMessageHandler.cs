using Blazored.SessionStorage;
using FinanceApp2.Shared.Extensions;
using FinanceApp2.Shared.Services.Responses;
using FinanceApp2.Web.Errors;
using FinanceApp2.Web.Helpers;
using FinanceApp2.Web.Settings;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FinanceApp2.Web.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly ILogger<JwtAuthorizationMessageHandler> _logger;
        private readonly string _authBaseUrl;
        private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

        public JwtAuthorizationMessageHandler(
            ISessionStorageService sessionStorage,
            ILogger<JwtAuthorizationMessageHandler> logger,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _sessionStorage = sessionStorage;
            _logger = logger;
            _authBaseUrl = applicationSettings.Value.AuthBaseUrl;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jwtToken = await _sessionStorage.GetItemAsync<string>("jwt_token");

            if (JwtTokenHelpers.IsTokenExpiredOrExpiring(jwtToken))
            {
                await _refreshSemaphore.WaitAsync(cancellationToken);
                try
                {
                    jwtToken = await _sessionStorage.GetItemAsync<string>("jwt_token");
                    if (JwtTokenHelpers.IsTokenExpiredOrExpiring(jwtToken))
                    {
                        jwtToken = await RefreshTokenAsync(cancellationToken);
                    }
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }

            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _refreshSemaphore.WaitAsync(cancellationToken);
                try
                {
                    jwtToken = await RefreshTokenAsync(cancellationToken);

                    if (!string.IsNullOrWhiteSpace(jwtToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }

            return response;
        }

        private async Task<string?> RefreshTokenAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(_authBaseUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, "/sessions/refresh");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                var response = await httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    await _sessionStorage.SetItemAsync("jwt_token", authResponse.AccessToken);

                    return authResponse.AccessToken;
                }
                else
                {
                    await _sessionStorage.RemoveItemAsync("jwt_token");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while refreshing token. ErrorCode: {ErrorCode}",
                    WebErrorCodes.REFRESH_TOKEN_ERROR);
                await _sessionStorage.RemoveItemAsync("jwt_token");
                return null;
            }
        }
    }

}
