using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Domain;
using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Data.Seed;

internal sealed class DatabaseInitializer(SalesDbContext db, IConfiguration config, TimeProvider clock, ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    /// <summary>Применяет миграции и создаёт воспроизводимый seed в пустой базе.</summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        if (!config.GetValue("Seed:Enabled", true)) return;
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(74839201)", ct);
        if (await db.SeedRuns.AnyAsync(ct)) return;
        if (await db.Sales.AnyAsync(ct) || await db.Managers.AnyAsync(ct))
            throw new InvalidOperationException("Database contains data but no seed marker; refusing to mix datasets.");
        var anchor = config["Seed:AnchorDate"] is { Length: > 0 } value
            ? DateRange.ParseDate(value, "Seed:AnchorDate") : DateRange.Today(clock);
        var seed = config.GetValue("Seed:RandomSeed", SeedGenerator.DefaultSeed);
        var data = SeedGenerator.Generate(anchor, seed);
        db.AddRange(data.Managers);
        db.AddRange(data.Customers);
        db.AddRange(data.Categories);
        db.AddRange(data.Products);
        db.AddRange(data.Sales);
        db.SeedRuns.Add(new SeedRun { Version = SeedGenerator.Version, RandomSeed = seed, AnchorDate = anchor });
        await db.SaveChangesAsync(ct);
        await db.Database.ExecuteSqlRawAsync("""
            SELECT setval(pg_get_serial_sequence('"Managers"','Id'), (SELECT MAX("Id") FROM "Managers"));
            SELECT setval(pg_get_serial_sequence('"Customers"','Id'), (SELECT MAX("Id") FROM "Customers"));
            SELECT setval(pg_get_serial_sequence('"Categories"','Id'), (SELECT MAX("Id") FROM "Categories"));
            SELECT setval(pg_get_serial_sequence('"Products"','Id'), (SELECT MAX("Id") FROM "Products"));
            SELECT setval(pg_get_serial_sequence('"Sales"','Id'), (SELECT MAX("Id") FROM "Sales"));
            SELECT setval(pg_get_serial_sequence('"SaleItems"','Id'), (SELECT MAX("Id") FROM "SaleItems"));
            """, ct);
        await transaction.CommitAsync(ct);
        db.ChangeTracker.Clear();
        logger.LogInformation("Seed v{Version}: {Count} sales, anchor {Anchor}", SeedGenerator.Version, data.Sales.Length, anchor);
    }
}
