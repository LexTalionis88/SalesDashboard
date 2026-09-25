using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Application.Abstractions.Services;

/// <summary>Предоставляет данные о продажах.</summary>
public interface ISalesService
{
    /// <summary>Возвращает страницу продаж за указанный период.</summary>
    Task<SalesDto> GetAsync(DateRange range, int limit, CancellationToken cancellationToken);
}
