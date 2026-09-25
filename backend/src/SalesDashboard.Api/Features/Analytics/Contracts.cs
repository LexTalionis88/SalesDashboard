using System.Globalization;
using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Features.Analytics;

internal static class Money
{
    internal static string Format(decimal value) => value.ToString("0.00##########################", CultureInfo.InvariantCulture);
    internal static string? Format(decimal? value) => value.HasValue ? Format(value.Value) : null;
}

public sealed record PeriodDto(DateOnly From, DateOnly To, string TimeZone = DateRange.ZoneId)
{
    internal static PeriodDto FromRange(DateRange range) => new(range.From, range.To);
}
public sealed record ManagerDto(int Id, string Name, string Team, string Position, bool IsActive, string Initials, string? AvatarUrl);
public sealed record MetricsDto(string Revenue, string Cost, string GrossProfit, long SalesCount, string? AverageCheck, decimal? Margin);
public sealed record KpisDto(string Revenue, string Cost, string GrossProfit, long SalesCount, string? AverageCheck, decimal? Margin, ManagerDto? BestManager);
public sealed record RankingDto(int? Rank, ManagerDto Manager, MetricsDto Metrics);
public sealed record DayDto(DateOnly Date, string Revenue, string GrossProfit, long SalesCount);
public sealed record CategoryDto(int Id, string Name, string Revenue, string GrossProfit, long SalesCount, long Quantity);
public sealed record ProductDto(int Id, string Name, int CategoryId, string Revenue, string GrossProfit, long SalesCount, long Quantity);
public sealed record ChangeDto(decimal? RevenuePercent, decimal? GrossProfitPercent, decimal? SalesCountPercent, decimal? AverageCheckPercent, decimal? MarginPoints);
public sealed record ComparisonDto(PeriodDto Period, MetricsDto Metrics, ChangeDto Change);
public sealed record DashboardDto(PeriodDto Period, string Currency, string RankingBy, KpisDto Kpis, RankingDto[] Ranking,
    DayDto[] Series, CategoryDto[] Categories, ProductDto[] TopProducts, ComparisonDto Comparison);

internal sealed record Metrics(decimal Revenue, decimal Cost, long SalesCount)
{
    internal decimal GrossProfit => Revenue - Cost;
    internal decimal? AverageCheck => SalesCount == 0 ? null : Revenue / SalesCount;
    internal decimal? Margin => Revenue == 0 ? null : GrossProfit / Revenue;
    internal MetricsDto ToDto() => new(Money.Format(Revenue), Money.Format(Cost), Money.Format(GrossProfit), SalesCount, Money.Format(AverageCheck), Margin);
    internal static decimal? PercentChange(decimal? current, decimal? previous) =>
        current is null || previous is null ? null : previous == 0 ? current == 0 ? 0 : null : (current - previous) / Math.Abs(previous.Value) * 100;
    internal ChangeDto ChangeFrom(Metrics other) => new(PercentChange(Revenue, other.Revenue), PercentChange(GrossProfit, other.GrossProfit),
        PercentChange(SalesCount, other.SalesCount), PercentChange(AverageCheck, other.AverageCheck), (Margin - other.Margin) * 100);
}
