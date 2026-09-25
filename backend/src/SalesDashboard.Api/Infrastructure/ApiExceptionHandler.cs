using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SalesDashboard.DataAccess.Infrastructure;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Преобразует исключение приложения в ProblemDetails с безопасным сообщением.</summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;
        if (exception is RequestValidationException validation)
            problem = new ValidationProblemDetails(new Dictionary<string, string[]> { [validation.Key] = [validation.Message] })
            { Status = 400, Title = "Некорректные параметры запроса" };
        else
        {
            logger.LogError(exception, "Request failed: {TraceId}", context.TraceIdentifier);
            problem = new ProblemDetails { Status = 500, Title = "Внутренняя ошибка сервера", Detail = "Повторите запрос позже." };
        }
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync<object>(problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
