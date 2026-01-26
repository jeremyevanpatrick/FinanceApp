using System.Net.Http.Headers;

namespace FinanceApp2.Web.Services
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly CustomAuthStateProvider _authProvider;

        public JwtAuthorizationMessageHandler(CustomAuthStateProvider authProvider)
        {
            _authProvider = authProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authProvider.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
