using Blazored.SessionStorage;
using FinanceApp2.Shared.Services;
using FinanceApp2.Shared.Settings;
using FinanceApp2.Web;
using FinanceApp2.Web.Data;
using FinanceApp2.Web.Services;
using FinanceApp2.Web.Settings;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.ClearProviders();
builder.Services.AddSingleton<ILoggerProvider, RemoteLoggerProvider>();

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<RemoteLoggingSettings>(builder.Configuration.GetSection("RemoteLogging"));
builder.Services.AddHttpClient("PublicApi", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped<NavigationMessageService>();
builder.Services.AddScoped<IBudgetClient, BudgetClient>();

builder.Services.AddBlazoredSessionStorage();

builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddScoped<IAuthClient, AuthClient>();

builder.Services.AddScoped<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("AuthenticatedApi", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
