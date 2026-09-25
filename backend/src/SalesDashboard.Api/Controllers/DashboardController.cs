using Microsoft.AspNetCore.Mvc;
using SalesDashboard.Application.Abstractions.Services;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Api.Controllers;

/// <summary>HTTP-контроллер аналитики панели управления.</summary>
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IAnalyticsService analytics, TimeProvider clock) : ControllerBase
{
    /// <summary>
    /// Возвращает агрегаты аналитики за выбранный период.
    /// </summary>
    [HttpGet]
    public Task<DashboardDto> GetAsync(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? preset,
        [FromQuery] string? rankingBy,
        CancellationToken cancellationToken)
        => analytics.GetAsync(
            DateRange.Parse(from, to, preset, clock), rankingBy ?? "grossProfit", cancellationToken);
}
