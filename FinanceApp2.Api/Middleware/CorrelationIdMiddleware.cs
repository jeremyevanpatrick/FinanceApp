namespace FinanceApp2.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? correlationIdString = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            Guid correlationIdGuid;

            if (!Guid.TryParse(correlationIdString, out correlationIdGuid))
            {
                correlationIdGuid = Guid.NewGuid();
            }

            context.Items["CorrelationId"] = correlationIdGuid.ToString();

            context.Response.Headers["X-Correlation-ID"] = correlationIdGuid.ToString();

            await _next(context);
        }
    }
}
