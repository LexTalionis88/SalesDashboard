using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesDashboard.DataAccess.Abstractions.Services;
using SalesDashboard.DataAccess.Data;
using SalesDashboard.DataAccess.Data.Seed;

namespace SalesDashboard.DataAccess.Infrastructure;

/// <summary>Содержит регистрацию зависимостей слоя доступа к данным.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Регистрирует контекст PostgreSQL и инициализатор схемы/seed.</summary>
    public static IServiceCollection AddSalesDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Sales") ?? throw new InvalidOperationException("ConnectionStrings:Sales is required.")));
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        return services;
    }
}
