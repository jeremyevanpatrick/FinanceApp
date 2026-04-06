using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Exceptions;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Services.Responses;
using System.Net;
using System.Net.Http.Json;

namespace FinanceApp2.Web.Helpers
{
    public class RequestHelper
    {
        private readonly HttpClient _httpClient;

        public RequestHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse?> GetAsync<TResponse>(string requestUrl, bool includeCredentials = false, int timeoutMs = 3000)
        {
            var response = await SendRequestAsync<object>(HttpMethod.Get, requestUrl, null, includeCredentials, timeoutMs);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUrl, TRequest? request, bool includeCredentials = false, int timeoutMs = 3000)
        {
            var response = await SendRequestAsync<TRequest>(HttpMethod.Post, requestUrl, request, includeCredentials, timeoutMs);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PostAsync<TRequest>(string requestUrl, TRequest? request, bool includeCredentials = false, int timeoutMs = 3000)
        {
            await SendRequestAsync<TRequest>(HttpMethod.Post, requestUrl, request, includeCredentials, timeoutMs);
        }

        public async Task PatchAsync<TRequest>(string requestUrl, TRequest? request, bool includeCredentials = false, int timeoutMs = 3000)
        {
            await SendRequestAsync<TRequest>(HttpMethod.Patch, requestUrl, request, includeCredentials, timeoutMs);
        }

        public async Task DeleteAsync<TRequest>(string requestUrl, TRequest? request, bool includeCredentials = false, int timeoutMs = 3000)
        {
            await SendRequestAsync<TRequest>(HttpMethod.Delete, requestUrl, request, includeCredentials, timeoutMs);
        }

        private async Task<HttpResponseMessage> SendRequestAsync<TRequest>(HttpMethod method, string requestUrl, TRequest? request, bool includeCredentials = false, int timeoutMs = 3000)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));

            var httpRequestMessage = new HttpRequestMessage(method, requestUrl);
            if (request != null)
            {
                httpRequestMessage.Content = JsonContent.Create<TRequest>(request);
            }

            if (includeCredentials)
            {
                httpRequestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            }

            var response = await _httpClient.SendAsync(httpRequestMessage, cts.Token);

            HandleErrors(response);

            return response;
        }

        private void HandleErrors(HttpResponseMessage response)
        {
            //check for recoverable error
            if (response.StatusCode >= HttpStatusCode.BadRequest &&
                response.StatusCode <= (HttpStatusCode)499)
            {
                ApiErrorResponse? errorResponse = null;
                try
                {
                    errorResponse = response.Content.ReadFromJsonAsync<ApiErrorResponse>().Result;
                }
                catch { }

                if (!string.IsNullOrWhiteSpace(errorResponse?.ErrorCode) && RecoverableErrorCodes.Any(e => e.ToString() == errorResponse.ErrorCode) && !string.IsNullOrWhiteSpace(errorResponse?.Detail))
                {
                    bool isUnauthorized = ApiErrorCodes.UNAUTHORIZED == errorResponse.ErrorCode;
                    throw new HttpRecoverableError(errorResponse.Detail, response.StatusCode, isUnauthorized);
                }
            }
            //check for unrecoverable error
            response.EnsureSuccessStatusCode();
        }

        private List<string> RecoverableErrorCodes { get; } = new()
        {
            ApiErrorCodes.INVALID_CREDENTIALS,
            ApiErrorCodes.INVALID_REQUEST_PARAMETERS,
            ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED,
            ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS,
            ApiErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE,
            ApiErrorCodes.ACCOUNT_LOCKED,
            ApiErrorCodes.AUTH_NO_LONGER_VALID,
            ApiErrorCodes.UNAUTHORIZED,
            ApiErrorCodes.FORBIDDEN,
            ApiErrorCodes.TOOMANYREQUESTS
        };

    }

}
