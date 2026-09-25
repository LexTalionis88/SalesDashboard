using System.Text.Json;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Data.Seed;
using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.Tests.Unit;

[TestFixture, Category("Unit")]
public sealed class SalesServiceTests
{
    private static readonly TimeProvider Clock = new FixedClock(new DateTimeOffset(2026, 3, 1, 0, 30, 0, TimeSpan.Zero));


}
