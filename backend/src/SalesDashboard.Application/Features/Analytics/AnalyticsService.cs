using Microsoft.EntityFrameworkCore;
using SalesDashboard.Application.Abstractions.Services;
using SalesDashboard.DataAccess.Data;
using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Application.Features.Analytics;

internal sealed class AnalyticsService(SalesDbContext db) : IAnalyticsService
{
    private IQueryable<Sale> Paid(DateRange range) => db.Sales.AsNoTracking()
        .Where(s => s.Status == SaleStatus.Paid && s.SoldAt >= range.StartUtc && s.SoldAt < range.EndUtc);

    private IQueryable<SaleAggregate> Totals(DateRange range) => Paid(range).Select(s => new SaleAggregate
    {
        Id = s.Id, ManagerId = s.ManagerId, SoldAt = s.SoldAt,
        Revenue = s.Items.Sum(i => i.UnitPrice * i.Quantity), Cost = s.Items.Sum(i => i.UnitCost * i.Quantity)
    });

    private async Task<Metrics> MetricsAsync(DateRange range, CancellationToken ct)
    {
        return await Totals(range).GroupBy(_ => 1).Select(g => new Metrics(g.Sum(x => x.Revenue), g.Sum(x => x.Cost), g.LongCount()))
            .SingleOrDefaultAsync(ct) ?? new Metrics(0, 0, 0);
    }

    /// <summary>Возвращает серверные KPI и аналитические срезы для выбранного периода.</summary>
    public async Task<DashboardDto> GetAsync(DateRange range, string rankingBy, CancellationToken ct)
    {
        ValidateRanking(rankingBy);
        var managerData = await LoadManagerDataAsync(range, rankingBy, ct);
        var series = await BuildSeriesAsync(range, ct);
        var categories = await LoadCategoriesAsync(range, ct);
        var products = await LoadProductsAsync(range, ct);
        var previous = await MetricsAsync(range.Previous, ct);
        return BuildDashboard(range, rankingBy, managerData, series, categories, products, previous);
    }

    internal static void ValidateRanking(string rankingBy)
    {
        if (rankingBy is not ("grossProfit" or "averageCheck"))
            throw new RequestValidationException("rankingBy", "????????? grossProfit ? averageCheck.");
    }

    private async Task<ManagerData> LoadManagerDataAsync(DateRange range, string rankingBy, CancellationToken ct)
    {
        var groups = await Totals(range).GroupBy(s => s.ManagerId)
            .Select(g => new { Id = g.Key, Revenue = g.Sum(s => s.Revenue), Cost = g.Sum(s => s.Cost), Count = g.LongCount() }).ToListAsync(ct);
        var managers = await db.Managers.AsNoTracking().OrderBy(m => m.Id)
            .Select(m => new ManagerDto(m.Id, m.Name, m.Team, m.Position, m.IsActive, m.Initials, m.AvatarUrl)).ToListAsync(ct);
        var byManager = groups.ToDictionary(g => g.Id, g => new Metrics(g.Revenue, g.Cost, g.Count));
        var rows = managers.Select(m => new ManagerRow(m, byManager.GetValueOrDefault(m.Id, new Metrics(0, 0, 0)))).ToArray();
        var ranked = rows.OrderByDescending(x => x.Metrics.SalesCount > 0)
            .ThenByDescending(x => rankingBy == "grossProfit" ? x.Metrics.GrossProfit : x.Metrics.AverageCheck)
            .ThenBy(x => x.Manager.Id).ToArray();
        var ranking = ranked.Select((x, i) => new RankingDto(x.Metrics.SalesCount == 0 ? null : i + 1, x.Manager, x.Metrics.ToDto())).ToArray();
        var best = rows.Where(x => x.Metrics.SalesCount > 0).OrderByDescending(x => x.Metrics.GrossProfit).ThenBy(x => x.Manager.Id).FirstOrDefault()?.Manager;
        var metrics = new Metrics(groups.Sum(g => g.Revenue), groups.Sum(g => g.Cost), groups.Sum(g => g.Count));
        return new ManagerData(metrics, best, ranking);
    }

    private async Task<DayDto[]> BuildSeriesAsync(DateRange range, CancellationToken ct)
    {
        var daily = await Totals(range).GroupBy(s => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(s.SoldAt, DateRange.ZoneId).Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Revenue), Cost = g.Sum(x => x.Cost), Count = g.LongCount() }).ToListAsync(ct);
        var days = daily.ToDictionary(x => DateOnly.FromDateTime(x.Date));
        return Enumerable.Range(0, range.Days).Select(i =>
        {
            var date = range.From.AddDays(i);
            return days.TryGetValue(date, out var d)
                ? new DayDto(date, Money.Format(d.Revenue), Money.Format(d.Revenue - d.Cost), d.Count)
                : new DayDto(date, "0.00", "0.00", 0);
        }).ToArray();
    }

    private async Task<CategoryDto[]> LoadCategoriesAsync(DateRange range, CancellationToken ct)
    {
        var items = Paid(range).SelectMany(s => s.Items);
        var rows = await items.GroupBy(i => new { i.Product.CategoryId, i.Product.Category.Name })
            .Select(g => new { Id = g.Key.CategoryId, Name = g.Key.Name, Revenue = g.Sum(i => i.UnitPrice * i.Quantity), Cost = g.Sum(i => i.UnitCost * i.Quantity), Count = g.Select(i => i.SaleId).Distinct().LongCount(), Quantity = g.Sum(i => (long)i.Quantity) })
            .OrderByDescending(g => g.Revenue).ThenBy(g => g.Id).ToListAsync(ct);
        return rows.Select(g => new CategoryDto(g.Id, g.Name, Money.Format(g.Revenue), Money.Format(g.Revenue - g.Cost), g.Count, g.Quantity)).ToArray();
    }

    private async Task<ProductDto[]> LoadProductsAsync(DateRange range, CancellationToken ct)
    {
        var items = Paid(range).SelectMany(s => s.Items);
        var rows = await items.GroupBy(i => new { i.ProductId, i.Product.Name, i.Product.CategoryId })
            .Select(g => new { Id = g.Key.ProductId, Name = g.Key.Name, CategoryId = g.Key.CategoryId, Revenue = g.Sum(i => i.UnitPrice * i.Quantity), Profit = g.Sum(i => (i.UnitPrice - i.UnitCost) * i.Quantity), Count = g.Select(i => i.SaleId).Distinct().LongCount(), Quantity = g.Sum(i => (long)i.Quantity) })
            .OrderByDescending(g => g.Profit).ThenBy(g => g.Id).Take(5).ToListAsync(ct);
        return rows.Select(g => new ProductDto(g.Id, g.Name, g.CategoryId, Money.Format(g.Revenue), Money.Format(g.Profit), g.Count, g.Quantity)).ToArray();
    }

    internal static DashboardDto BuildDashboard(DateRange range, string rankingBy, ManagerData managerData, DayDto[] series, CategoryDto[] categories, ProductDto[] products, Metrics previous)
    {
        var metrics = managerData.Metrics;
        return new DashboardDto(PeriodDto.FromRange(range), "RUB", rankingBy,
            new KpisDto(Money.Format(metrics.Revenue), Money.Format(metrics.Cost), Money.Format(metrics.GrossProfit), metrics.SalesCount,
                Money.Format(metrics.AverageCheck), metrics.Margin, managerData.Best), managerData.Ranking, series, categories, products,
            new ComparisonDto(PeriodDto.FromRange(range.Previous), previous.ToDto(), metrics.ChangeFrom(previous)));
    }



    internal sealed record ManagerData(Metrics Metrics, ManagerDto? Best, RankingDto[] Ranking);
    private sealed record ManagerRow(ManagerDto Manager, Metrics Metrics);
    private sealed class SaleAggregate
    {
        public int Id { get; init; }
        public int ManagerId { get; init; }
        public DateTime SoldAt { get; init; }
        public decimal Revenue { get; init; }
        public decimal Cost { get; init; }
    }
}

