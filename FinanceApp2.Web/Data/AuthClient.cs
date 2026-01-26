using Blazored.SessionStorage;
using FinanceApp2.Shared.Data;
using FinanceApp2.Shared.Helpers;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Shared.Services.Responses;
using FinanceApp2.Web.Services;
using FinanceApp2.Web.Services.Requests;
using FinanceApp2.Web.Settings;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Web.Data
{
    public class AuthClient : IAuthClient
    {
        private readonly RequestHelper _requestHelperPublic;
        private readonly RequestHelper _requestHelperAuthenticated;
        private readonly string _authBaseUrl;

        private readonly ISessionStorageService _sessionStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthClient(
            IHttpClientFactory httpClientFactory,
            ISessionStorageService sessionStorage,
            AuthenticationStateProvider authStateProvider,
            IOptions<ApplicationSettings> applicationSettings,
            ILogger<BudgetClient> logger)
        {
            _sessionStorage = sessionStorage;
            _authStateProvider = authStateProvider;
            _requestHelperPublic = new RequestHelper(httpClientFactory.CreateClient("PublicApi"), logger);
            _requestHelperAuthenticated = new RequestHelper(httpClientFactory.CreateClient("AuthenticatedApi"), logger);
            _authBaseUrl = applicationSettings.Value.AuthBaseUrl;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/login";

            LoginWebRequest request = new LoginWebRequest(email, password);
            AuthResponse response = await _requestHelperPublic.PostRequestAsync<LoginWebRequest, AuthResponse>(requestUrl, request, null, 9000);

            await _sessionStorage.SetItemAsync("authToken", response.Token);
            await _sessionStorage.SetItemAsync("userId", response.UserId);
            await _sessionStorage.SetItemAsync("userEmail", response.Email);

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.Token);

            return response;
        }

        public async Task RegisterAsync(string email, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/register";
            RegisterWebRequest request = new RegisterWebRequest(email, password);
            await _requestHelperPublic.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            string requestUrl = $"{_authBaseUrl}/auth/resendconfirmationemail";
            ResendConfirmationEmailRequest request = new ResendConfirmationEmailRequest(email);
            await _requestHelperPublic.PostRequestNoResponseAsync<ResendConfirmationEmailRequest>(requestUrl, request, null, 9000);
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            string requestUrl = $"{_authBaseUrl}/auth/confirmemail";
            ConfirmEmailRequest request = new ConfirmEmailRequest(userId, token);
            await _requestHelperPublic.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangeEmailAsync(string newEmail, string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changeemail";
            ChangeEmailRequest request = new ChangeEmailRequest(newEmail, password);
            await _requestHelperAuthenticated.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangeEmailConfirmationAsync(string userId, string newEmail, string token)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changeemailconfirmation";
            ChangeEmailConfirmationRequest request = new ChangeEmailConfirmationRequest(userId, newEmail, token);
            await _requestHelperPublic.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ChangePasswordAsync(string existingPassword, string newPassword)
        {
            string requestUrl = $"{_authBaseUrl}/auth/changepassword";
            ChangePasswordRequest request = new ChangePasswordRequest(existingPassword, newPassword);
            await _requestHelperAuthenticated.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            string requestUrl = $"{_authBaseUrl}/auth/forgotpassword";
            ForgotPasswordRequest request = new ForgotPasswordRequest(email);
            await _requestHelperPublic.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task ResetPasswordAsync(string email, string resetCode, string newPassword)
        {
            string requestUrl = $"{_authBaseUrl}/auth/resetpassword";
            ResetPasswordRequest request = new ResetPasswordRequest(email, resetCode, newPassword);
            await _requestHelperPublic.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task DeleteAccountAsync(string password)
        {
            string requestUrl = $"{_authBaseUrl}/auth/delete";
            DeleteAccountRequest request = new DeleteAccountRequest(password);
            await _requestHelperAuthenticated.PostRequestNoResponseAsync(requestUrl, request, null, 9000);
        }

        public async Task LogoutAsync()
        {
            string requestUrl = $"{_authBaseUrl}/auth/logout";
            await _requestHelperAuthenticated.PostRequestNoResponseAsync<object>(requestUrl, new{ }, null, 9000);

            await _sessionStorage.RemoveItemAsync("authToken");
            await _sessionStorage.RemoveItemAsync("userId");
            await _sessionStorage.RemoveItemAsync("userEmail");

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _sessionStorage.GetItemAsync<string>("authToken");
        }

        public async Task<string> GetUserEmailAsync()
        {
            return await _sessionStorage.GetItemAsync<string>("userEmail");
        }
    }
}