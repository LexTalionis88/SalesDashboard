using System.Text.Json;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Data.Seed;
using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Tests.Unit;

[TestFixture, Category("Unit")]
public sealed class AnalyticsServiceTests
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

    [TestCase("grossProfit")]
        [TestCase("averageCheck")]
        public void Analytics_accepts_supported_ranking(string rankingBy)
        {
            Assert.DoesNotThrow(() => AnalyticsService.ValidateRanking(rankingBy));
        }

    [Test]
        public void Analytics_rejects_unknown_ranking()
        {
            var exception = Assert.Throws<RequestValidationException>(() => AnalyticsService.ValidateRanking("revenue"));
            Assert.That(exception!.Message, Does.Contain("rankingBy").Or.Contain("grossProfit"));
        }

    [Test]
        public void Analytics_builds_dashboard_from_calculated_parts()
        {
            var range = DateRange.Parse("2026-03-01", "2026-03-02", null, Clock);
            var current = new Metrics(500, 320, 2);
            var previous = new Metrics(250, 160, 1);
            var manager = new ManagerDto(1, "Анна Соколова", "Команда", "Менеджер", true, "АС", null);
            var result = AnalyticsService.BuildDashboard(
                range,
                "grossProfit",
                new AnalyticsService.ManagerData(current, manager, [new RankingDto(1, manager, current.ToDto())]),
                [new DayDto(range.From, "500.00", "180.00", 2)],
                [new CategoryDto(1, "Дроны", "500.00", "180.00", 2, 2)],
                [new ProductDto(1, "Модель", 1, "500.00", "180.00", 2, 2)],
                previous);
    
            Assert.Multiple(() =>
            {
                Assert.That(result.Kpis.Revenue, Is.EqualTo("500.00"));
                Assert.That(result.Kpis.BestManager, Is.EqualTo(manager));
                Assert.That(result.Ranking, Has.Length.EqualTo(1));
                Assert.That(result.Series, Has.Length.EqualTo(1));
                Assert.That(result.Categories, Has.Length.EqualTo(1));
                Assert.That(result.TopProducts, Has.Length.EqualTo(1));
                Assert.That(result.Comparison.Metrics.Revenue, Is.EqualTo("250.00"));
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
}
