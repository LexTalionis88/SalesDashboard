using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SalesDashboard.Application.Abstractions.Services;
using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Api.Controllers;

/// <summary>HTTP-контроллер истории продаж.</summary>
[ApiController]
[Route("api/sales")]
public sealed class SalesController(ISalesService sales, TimeProvider clock) : ControllerBase
{
    /// <summary>Возвращает продажи за выбранный период.</summary>
    [HttpGet]
    public Task<SalesDto> GetAsync([FromQuery] string? from, [FromQuery] string? to, [FromQuery] string? preset, [FromQuery] string? limit, CancellationToken cancellationToken)
    {
        var parsedLimit = 20;
        if (limit is not null && !int.TryParse(limit, NumberStyles.None, CultureInfo.InvariantCulture, out parsedLimit))
            throw new RequestValidationException("limit", "Требуется целое число от 1 до 100.");
        return sales.GetAsync(DateRange.Parse(from, to, preset, clock), parsedLimit, cancellationToken);
    }
}
