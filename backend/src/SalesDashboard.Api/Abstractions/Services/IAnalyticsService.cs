using SalesDashboard.Api.Features.Analytics;
using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Abstractions.Services;

/// <summary>Предоставляет серверную аналитику dashboard.</summary>
public interface IAnalyticsService
{
    /// <summary>Возвращает KPI, рейтинг, динамику, категории, товары и сравнение периодов.</summary>
    Task<DashboardDto> GetAsync(DateRange range, string rankingBy, CancellationToken cancellationToken);
}
