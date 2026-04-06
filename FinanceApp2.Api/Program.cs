using FinanceApp2.Api.Data.Context;
using FinanceApp2.Api.Data.Repositories;
using FinanceApp2.Api.Helpers;
using FinanceApp2.Api.Middleware;
using FinanceApp2.Api.Models;
using FinanceApp2.Api.Services.Application;
using FinanceApp2.Api.Services.Background;
using FinanceApp2.Api.Services.Queues;
using FinanceApp2.Api.Settings;
using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Services;
using FinanceApp2.Shared.Services.Queues;
using FinanceApp2.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Services.Responses;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IExternalScopeProvider, LoggerExternalScopeProvider>();

builder.Services.AddSingleton<ILoggerProvider, RemoteLoggerProvider>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LoggingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LoggingDb")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<ILogProcessorQueue, LogProcessorQueue>();
builder.Services.AddSingleton<IEmailSenderQueue, EmailSenderQueue>();

builder.Services.AddScoped<ILoggingRepository, LoggingRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<IBudgetAppService, BudgetAppService>();
builder.Services.AddScoped<IAuthAppService, AuthAppService>();

builder.Services.AddSingleton<BudgetsLinkHelper>();
builder.Services.AddSingleton<SessionsLinkHelper>();
builder.Services.AddSingleton<EmailConfirmationRequestsLinkHelper>();
builder.Services.AddSingleton<EmailChangeRequestsLinkHelper>();
builder.Services.AddSingleton<PasswordResetRequestsLinkHelper>();
builder.Services.AddSingleton<UsersLinkHelper>();

builder.Services.Configure<RemoteLoggingSettings>(builder.Configuration.GetSection("RemoteLogging"));
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("Client"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<DataCleanupSettings>(builder.Configuration.GetSection("DataCleanup"));

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new ResponseErrorItem
            {
                Field = x.Key,
                Messages = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
            }).ToList();

        string title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest);

        var apiErrorResponse = new ApiErrorResponse
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Detail = "Validation failed",
            ErrorCode = ApiErrorCodes.INVALID_REQUEST_PARAMETERS.ToString(),
            Errors = errors,
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(apiErrorResponse);
    };
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized);

            return context.Response.WriteAsJsonAsync(new ApiErrorResponse
            {
                Title = title,
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Your session has expired. Please sign in again.",
                ErrorCode = ApiErrorCodes.UNAUTHORIZED.ToString(),
                Instance = context.HttpContext.Request.Path
            });
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            string title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden);

            return context.Response.WriteAsJsonAsync(new ApiErrorResponse
            {
                Title = title,
                Status = StatusCodes.Status403Forbidden,
                Detail = "You do not have permission to access this resource.",
                ErrorCode = ApiErrorCodes.FORBIDDEN.ToString(),
                Instance = context.HttpContext.Request.Path
            });
        }
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("messaging-endpoints-global", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(60);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });

    options.AddPolicy("public-token-refresh-endpoint", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var metadata) ? (int?)metadata.TotalSeconds : null;
        if (retryAfterSeconds.HasValue)
        {
            context.HttpContext.Response.Headers["Retry-After"] = retryAfterSeconds.Value.ToString();
        }

        string title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status429TooManyRequests);

        await context.HttpContext.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            Title = title,
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Rate limit exceeded. Try again later.",
            ErrorCode = ApiErrorCodes.TOOMANYREQUESTS.ToString(),
            Instance = context.HttpContext.Request.Path
        });
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHostedService<ErrorLoggingService>();
builder.Services.AddHostedService<DataCleanupService>();
builder.Services.AddHostedService<EmailSenderService>();

var clientSettings = builder.Configuration.GetSection("Client").Get<ClientSettings>()!;

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins(clientSettings.Host)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseCors("BlazorPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
