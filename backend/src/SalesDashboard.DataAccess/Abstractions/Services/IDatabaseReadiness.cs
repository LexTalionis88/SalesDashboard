namespace SalesDashboard.DataAccess.Abstractions.Services;

/// <summary>Проверяет доступность хранилища данных.</summary>
public interface IDatabaseReadiness
{
    /// <summary>Возвращает признак доступности базы данных.</summary>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}
