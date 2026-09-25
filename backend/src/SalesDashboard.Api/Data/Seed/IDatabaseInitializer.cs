namespace SalesDashboard.Api.Data.Seed;

/// <summary>Применяет миграции и выполняет идемпотентное первоначальное заполнение базы.</summary>
public interface IDatabaseInitializer
{
    /// <summary>Готовит схему базы и создаёт seed при первом запуске.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
