using System.Text.Json;
using SalesDashboard.DataAccess.Data.Seed;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Tests.Unit;

[TestFixture, Category("Unit")]
public sealed class BusinessRulesTests
{
    private static readonly TimeProvider Clock = new FixedClock(new DateTimeOffset(2026, 3, 1, 0, 30, 0, TimeSpan.Zero));

    [Test]
    public void Metrics_match_documented_example()
    {
        var m = new Metrics(500, 320, 2);
        Assert.Multiple(() =>
        {
            Assert.That(m.GrossProfit, Is.EqualTo(180));
            Assert.That(m.AverageCheck, Is.EqualTo(250));
            Assert.That(m.Margin, Is.EqualTo(.36m));
            Assert.That(m.ToDto().Revenue, Is.EqualTo("500.00"));
        });
    }

    [Test]
    public void Empty_and_zero_revenue_do_not_divide_by_zero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Metrics(0, 0, 0).AverageCheck, Is.Null);
            Assert.That(new Metrics(0, 100, 1).Margin, Is.Null);
            Assert.That(new Metrics(0, 100, 1).AverageCheck, Is.Zero);
            Assert.That(new Metrics(0, 100, 1).GrossProfit, Is.EqualTo(-100));
        });
    }

    [TestCase(0, 0, 0)]
    [TestCase(100, 50, 100)]
    [TestCase(-50, -100, 50)]
    [TestCase(50, -100, 150)]
    public void Changes_handle_negative_base(decimal current, decimal previous, decimal expected) =>
        Assert.That(Metrics.PercentChange(current, previous), Is.EqualTo(expected));

    [Test]
    public void Missing_comparison_base_returns_null()
    {
        Assert.That(Metrics.PercentChange(50, 0), Is.Null);
        Assert.That(Metrics.PercentChange(null, 50), Is.Null);
    }

    [TestCase("today", "2026-03-01", "2026-03-01")]
    [TestCase("last7Days", "2026-02-23", "2026-03-01")]
    [TestCase("last30Days", "2026-01-31", "2026-03-01")]
    [TestCase("thisMonth", "2026-03-01", "2026-03-01")]
    [TestCase("lastMonth", "2026-02-01", "2026-02-28")]
    public void Presets_use_business_calendar(string preset, string from, string to)
    {
        var range = DateRange.Parse(null, null, preset, Clock);
        Assert.That(range.From.ToString("yyyy-MM-dd"), Is.EqualTo(from));
        Assert.That(range.To.ToString("yyyy-MM-dd"), Is.EqualTo(to));
    }

    [Test]
    public void Dates_use_moscow_midnight_and_equal_previous_period()
    {
        var range = DateRange.Parse("2026-09-10", "2026-09-16", null, Clock);
        Assert.Multiple(() =>
        {
            Assert.That(range.StartUtc, Is.EqualTo(new DateTime(2026, 9, 9, 21, 0, 0, DateTimeKind.Utc)));
            Assert.That(range.EndUtc, Is.EqualTo(new DateTime(2026, 9, 16, 21, 0, 0, DateTimeKind.Utc)));
            Assert.That(range.Previous.From, Is.EqualTo(new DateOnly(2026, 9, 3)));
            Assert.That(range.Previous.To, Is.EqualTo(new DateOnly(2026, 9, 9)));
        });
    }

    [TestCase("2026-01-01", null, null)]
    [TestCase("bad", "2026-01-01", null)]
    [TestCase("2026-02-30", "2026-03-01", null)]
    [TestCase("2026-03-02", "2026-03-01", null)]
    [TestCase("1900-01-01", "2026-03-01", null)]
    [TestCase("2026-03-01", "2026-03-01", "today")]
    [TestCase(null, null, "unknown")]
    public void Invalid_periods_are_rejected(string? from, string? to, string? preset) =>
        Assert.Throws<RequestValidationException>(() => DateRange.Parse(from, to, preset, Clock));

    [Test]
    public void Seed_is_reproducible_and_contains_required_scenarios()
    {
        var anchor = new DateOnly(2026, 9, 25);
        var first = SeedGenerator.Generate(anchor, SeedGenerator.DefaultSeed);
        var second = SeedGenerator.Generate(anchor, SeedGenerator.DefaultSeed);
        Assert.That(JsonSerializer.Serialize(first), Is.EqualTo(JsonSerializer.Serialize(second)));
        Assert.Multiple(() =>
        {
            Assert.That(first.Managers, Has.Length.EqualTo(20));
            Assert.That(first.Customers, Has.Length.EqualTo(75));
            Assert.That(first.Categories, Has.Length.EqualTo(6));
            Assert.That(first.Products, Has.Length.EqualTo(48));
            Assert.That(first.Sales, Has.Length.EqualTo(3000));
            Assert.That(first.Sales.All(s => s.Items.Count > 0), Is.True);
            Assert.That(first.Sales.Select(s => s.Status).Distinct().Count(), Is.EqualTo(3));
            Assert.That(first.Sales.Min(s => s.SoldAt), Is.EqualTo(DateRange.ToUtc(anchor.AddMonths(-12).AddDays(1))));
            Assert.That(first.Sales.Any(s => s.ManagerId == 18 && s.SoldAt >= DateRange.ToUtc(anchor.AddDays(-29))), Is.False);
            Assert.That(first.Sales.Any(s => s.Items.Any(i => i.UnitCost > i.UnitPrice)), Is.True);
        });
    }
}

public sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

