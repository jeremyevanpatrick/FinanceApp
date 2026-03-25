using Blazored.SessionStorage;
using FinanceApp2.Shared.Services.DTOs;
using FinanceApp2.Shared.Services.Requests;
using FinanceApp2.Shared.Services.Responses;
using FinanceApp2.Web.Helpers;
using FinanceApp2.Web.Services;
using FinanceApp2.Web.Services.Requests;
using FinanceApp2.Web.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace FinanceApp2.Web.Data
{
    public class AuthClient : BaseClient, IAuthClient
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
            ILogger<AuthClient> logger,
            NavigationManager navigationManager,
            NavigationMessageService navigationMessageService)
            : base(logger, navigationManager, navigationMessageService)
        {
            _sessionStorage = sessionStorage;
            _authStateProvider = authStateProvider;
            _requestHelperPublic = new RequestHelper(httpClientFactory.CreateClient("PublicApi"));
            _requestHelperAuthenticated = new RequestHelper(httpClientFactory.CreateClient("AuthenticatedApi"));
            _authBaseUrl = applicationSettings.Value.AuthBaseUrl;
        }

        public Task<BaseResult> RegisterAsync(string email, string password) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/users";
                RegisterWebRequest request = new RegisterWebRequest(email, password);
                await _requestHelperPublic.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> DeleteAccountAsync(string password) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/users/me";
                DeleteAccountRequest request = new DeleteAccountRequest(password);
                await _requestHelperAuthenticated.DeleteAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ResendConfirmationEmailAsync(string email) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/email-confirmations";
                ResendConfirmationEmailRequest request = new ResendConfirmationEmailRequest(email);
                await _requestHelperPublic.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ConfirmEmailAsync(string userId, string token) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/email-confirmations/confirm";
                ConfirmEmailRequest request = new ConfirmEmailRequest(userId, token);
                await _requestHelperPublic.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ChangeEmailAsync(string newEmail, string password) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/users/me/email";
                ChangeEmailRequest request = new ChangeEmailRequest(newEmail, password);
                await _requestHelperAuthenticated.PatchAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ChangeEmailConfirmationAsync(string userId, string newEmail, string token) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/email-change-confirmations";
                ChangeEmailConfirmationRequest request = new ChangeEmailConfirmationRequest(userId, newEmail, token);
                await _requestHelperAuthenticated.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ChangePasswordAsync(string existingPassword, string newPassword) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/users/me/password";
                ChangePasswordRequest request = new ChangePasswordRequest(existingPassword, newPassword);
                await _requestHelperAuthenticated.PatchAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ForgotPasswordAsync(string email) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/password-reset-requests";
                ForgotPasswordRequest request = new ForgotPasswordRequest(email);
                await _requestHelperPublic.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> ResetPasswordAsync(string email, string resetCode, string newPassword) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/password-resets";
                ResetPasswordRequest request = new ResetPasswordRequest(email, resetCode, newPassword);
                await _requestHelperPublic.PostAsync(requestUrl, request, false, 9000);
            });

        public Task<BaseResult> LoginAsync(string email, string password) =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/sessions";

                LoginWebRequest request = new LoginWebRequest(email, password);
                AuthResponse response = await _requestHelperPublic.PostAsync<LoginWebRequest, AuthResponse>(requestUrl, request, true, 9000);

                await _sessionStorage.SetItemAsync("jwt_token", response.AccessToken);
                await _sessionStorage.SetItemAsync("user_id", response.UserId);
                await _sessionStorage.SetItemAsync("user_email", response.Email);

                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.AccessToken);
            });

        public Task<BaseResult> LogoutAsync() =>
            ExecuteAsync(async () =>
            {
                string requestUrl = $"{_authBaseUrl}/sessions";

                await _requestHelperAuthenticated.DeleteAsync<object>(requestUrl, null, true, 9000);

                await _sessionStorage.RemoveItemAsync("jwt_token");
                await _sessionStorage.RemoveItemAsync("user_id");
                await _sessionStorage.RemoveItemAsync("user_email");

                ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            });

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetTokenAsync()
        {
            return await _sessionStorage.GetItemAsync<string>("jwt_token");
        }

        public async Task<string> GetUserEmailAsync()
        {
            return await _sessionStorage.GetItemAsync<string>("user_email");
        }
    }
}