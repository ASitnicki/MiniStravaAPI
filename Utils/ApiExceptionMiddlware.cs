using System.Net;
using System.Text.Json;

namespace MiniStrava.Utils
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                await Handle(ctx, ex);
            }
        }

        private static Task Handle(HttpContext ctx, Exception ex)
        {
            var (status, message) = ex switch
            {
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, ex.Message),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
                ArgumentException => ((int)HttpStatusCode.BadRequest, ex.Message),
                _ => ((int)HttpStatusCode.InternalServerError, "Internal server error.")
            };

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new { error = message, statusCode = status });
            return ctx.Response.WriteAsync(payload);
        }
    }

    public static class ApiExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptions(this IApplicationBuilder app)
            => app.UseMiddleware<ApiExceptionMiddleware>();
    }
}
