using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace FinanceApp2.Shared.Helpers
{
    public class RequestHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public RequestHelper(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TResponse> GetRequestAsync<TResponse>(string requestUrl, IDictionary<string, string>? headers = null, int timeoutMs = 10000)
        {
            string requestString = "";
            string responseString = "";
            string userId = "";//get from header

            /*if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }*/

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var response = await _httpClient.GetAsync(requestUrl, cts.Token);

                responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseObject = JsonSerializer.Deserialize<TResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (responseObject == null)
                    {
                        throw new Exception($"Invalid response from service");
                    }

                    return responseObject;
                }

                throw new HttpRequestException($"Request error {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.S-00001", ex, "Request error", new Dictionary<string, string> {
                    { "Url", requestUrl },
                    { "UserId", userId },
                    { "Response", responseString }
                });
                throw;
            }
        }

        public async Task<TResponse> PostRequestAsync<TRequest, TResponse>(string requestUrl, TRequest request, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            return await PostAsync<TRequest, TResponse>(requestUrl, request, headers, timeoutMs, true);
        }

        public async Task PostRequestNoResponseAsync<TRequest>(string requestUrl, TRequest request, IDictionary<string, string>? headers = null, int timeoutMs = 3000)
        {
            await PostAsync<TRequest, bool>(requestUrl, request, headers, timeoutMs, false);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUrl, TRequest request, IDictionary<string, string>? headers = null, int timeoutMs = 3000, bool returnResponse = true)
        {
            string requestString = "";
            string responseString = "";
            string userId = "";//get from header

            /*if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }*/

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var response = await _httpClient.PostAsJsonAsync(requestUrl, request, cts.Token);

                responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (!returnResponse)
                    {
                        return default;
                    }

                    var responseObject = JsonSerializer.Deserialize<TResponse>(responseString, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true });
                    if (responseObject == null)
                    {
                        throw new Exception($"Invalid response from service");
                    }

                    return responseObject;
                }
                
                throw new HttpRequestException(response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary("FI2.S-00002", ex, "Request error", new Dictionary<string, string> {
                    { "Url", requestUrl },
                    { "UserId", userId },
                    { "Request", requestString },
                    { "Response", responseString }
                });
                throw;
            }
        }

    }

}
