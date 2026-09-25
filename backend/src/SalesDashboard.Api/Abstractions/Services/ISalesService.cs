using SalesDashboard.Api.Features.Sales;
using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Abstractions.Services;

/// <summary>Предоставляет ограниченную историю продаж.</summary>
public interface ISalesService
{
    /// <summary>Возвращает последние продажи за указанный период.</summary>
    Task<SalesDto> GetAsync(DateRange range, int limit, CancellationToken cancellationToken);
}
