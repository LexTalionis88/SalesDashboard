using System.Text.Json;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Data.Seed;
using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Tests.Unit;

[TestFixture, Category("Unit")]
public sealed class DateRangeTests
{
    private static readonly TimeProvider Clock = new FixedClock(new DateTimeOffset(2026, 3, 1, 0, 30, 0, TimeSpan.Zero));

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
}
