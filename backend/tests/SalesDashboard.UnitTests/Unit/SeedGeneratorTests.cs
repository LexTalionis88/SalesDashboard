using System.Text.Json;
using SalesDashboard.Application.Features.Analytics;
using SalesDashboard.Application.Features.Sales;
using SalesDashboard.DataAccess.Data.Seed;
using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.UnitTests;

[TestFixture, Category("Unit")]
public sealed class SeedGeneratorTests
{
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

