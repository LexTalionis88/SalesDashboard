using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Application.Abstractions.Services;

/// <summary>Предоставляет серверную аналитику панели управления.</summary>
public interface IAnalyticsService
{
    /// <summary>Возвращает KPI, рейтинг, динамику, категории, товары и сравнение периодов.</summary>
    Task<DashboardDto> GetAsync(DateRange range, string rankingBy, CancellationToken cancellationToken);
}
