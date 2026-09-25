using Microsoft.AspNetCore.Mvc;
using SalesDashboard.DataAccess.Abstractions.Services;

namespace SalesDashboard.Api.Controllers;

/// <summary>Контроллер проверок состояния приложения.</summary>
[ApiController]
[Route("api/health")]
public sealed class HealthController(IDatabaseReadiness readiness) : ControllerBase
{
    /// <summary>Проверяет доступность базы данных.</summary>
    [HttpGet("ready")]
    public async Task<IActionResult> ReadyAsync(CancellationToken cancellationToken)
        => await readiness.CanConnectAsync(cancellationToken)
            ? Ok(new { status = "ready" })
            : Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Database unavailable");
}
