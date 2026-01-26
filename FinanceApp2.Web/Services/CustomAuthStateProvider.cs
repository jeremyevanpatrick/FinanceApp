using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace FinanceApp2.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(
            ISessionStorageService sessionStorage,
            HttpClient httpClient)
        {
            _sessionStorage = sessionStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _sessionStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.Select(kvp =>
            {
                var type = kvp.Key switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "emailaddress" => ClaimTypes.Email,
                    _ => kvp.Key
                };

                return new Claim(type, kvp.Value.ToString() ?? "");
            });
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _sessionStorage.GetItemAsync<string>("authToken");
        }
    }
}
