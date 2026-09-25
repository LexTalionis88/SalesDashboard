using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Features.Analytics;

/// <summary>Предоставляет серверную аналитику dashboard.</summary>
public interface IAnalyticsService
{
    /// <summary>Возвращает KPI, рейтинг, динамику, категории, товары и сравнение периодов.</summary>
    Task<DashboardDto> GetAsync(DateRange range, string rankingBy, CancellationToken cancellationToken);
}
