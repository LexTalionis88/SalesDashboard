using Microsoft.Extensions.DependencyInjection;
using SalesDashboard.Application.Abstractions.Services;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.Application.Features.Sales;

namespace SalesDashboard.Application.Infrastructure;

/// <summary>Методы регистрации сервисов бизнес-логики.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует прикладные сервисы в контейнере зависимостей.
    /// </summary>
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISalesService, SalesService>();
        return services;
    }
}
