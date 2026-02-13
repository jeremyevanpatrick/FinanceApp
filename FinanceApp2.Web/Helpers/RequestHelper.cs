using FinanceApp2.Shared.Helpers;
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
                ProblemResponse? problemResponse = null;
                try
                {
                    problemResponse = response.Content.ReadFromJsonAsync<ProblemResponse>().Result;
                }
                catch { }

                if (!string.IsNullOrWhiteSpace(problemResponse?.ErrorCode) && RecoverableErrorCodes.Any(e => e.ToString() == problemResponse.ErrorCode) && !string.IsNullOrWhiteSpace(problemResponse?.Detail))
                {
                    bool isUnauthorized = ResponseErrorCodes.UNAUTHORIZED.ToString() == problemResponse.ErrorCode;
                    throw new HttpRecoverableError(problemResponse.Detail, response.StatusCode, isUnauthorized);
                }
            }
            //check for unrecoverable error
            response.EnsureSuccessStatusCode();
        }

        private List<ResponseErrorCodes> RecoverableErrorCodes { get; } = new()
        {
            ResponseErrorCodes.INVALID_CREDENTIALS,
            ResponseErrorCodes.INVALID_REQUEST_PARAMETERS,
            ResponseErrorCodes.TOKEN_INVALID_OR_EXPIRED,
            ResponseErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS,
            ResponseErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE,
            ResponseErrorCodes.ACCOUNT_LOCKED,
            ResponseErrorCodes.AUTH_NO_LONGER_VALID,
            ResponseErrorCodes.UNAUTHORIZED,
            ResponseErrorCodes.FORBIDDEN
        };

    }

}
