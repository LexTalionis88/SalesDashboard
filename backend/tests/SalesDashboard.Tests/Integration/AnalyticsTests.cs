using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SalesDashboard.Api.Data;
using SalesDashboard.Api.Data.Seed;
using SalesDashboard.Api.Domain;
using SalesDashboard.Api.Features.Analytics;
using SalesDashboard.Api.Features.Sales;
using SalesDashboard.Api.Infrastructure;
using Testcontainers.PostgreSql;

namespace SalesDashboard.Tests.Integration;

[TestFixture, Category("Integration"), NonParallelizable]
public sealed class AnalyticsTests
{
    private PostgreSqlContainer postgres = null!;
    private ApiFactory factory = null!;
    private HttpClient client = null!;
    private readonly DateRange range = new(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 16));
    private const string Query = "from=2026-09-10&to=2026-09-16";

    [OneTimeSetUp]
    public async Task Start()
    {
        postgres = new PostgreSqlBuilder("postgres:17.6-alpine").Build();
        await postgres.StartAsync();
        factory = new ApiFactory(postgres.GetConnectionString());
        client = factory.CreateClient();
    }

    [SetUp]
    public async Task Reset()
    {
        await using var db = Db();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE \"SaleItems\", \"Sales\", \"Products\", \"Categories\", \"Customers\", \"Managers\", \"SeedRuns\" RESTART IDENTITY CASCADE");
        db.Managers.AddRange(Enumerable.Range(1, 4).Select(i => new Manager { Id = i, Name = $"Manager {i}", Initials = "M", IsActive = i != 2 }));
        db.Customers.Add(new Customer { Id = 1, Name = "Customer", Company = "Company", Segment = "SMB" });
        db.Categories.AddRange(new Category { Id = 1, Name = "A" }, new Category { Id = 2, Name = "B" });
        db.Products.AddRange(new Product { Id = 1, Name = "P1", Sku = "P1", CategoryId = 1 }, new Product { Id = 2, Name = "P2", Sku = "P2", CategoryId = 2 });
        await db.SaveChangesAsync();
    }

    private SalesDbContext Db() => new(new DbContextOptionsBuilder<SalesDbContext>().UseNpgsql(postgres.GetConnectionString()).Options);
    private static Sale Sale(int manager, DateTime date, SaleStatus status, params (int Product, int Quantity, decimal Price, decimal Cost)[] items) => new()
    {
        ManagerId = manager, CustomerId = 1, SoldAt = date, Status = status,
        Items = items.Select(i => new SaleItem { ProductId = i.Product, Quantity = i.Quantity, UnitPrice = i.Price, UnitCost = i.Cost }).ToList()
    };

    [Test]
    public async Task Aggregates_respect_status_boundaries_and_distinct_sales()
    {
        await using var db = Db();
        db.Sales.AddRange(
            Sale(1, range.StartUtc, SaleStatus.Paid, (1, 1, 100, 60), (2, 1, 100, 60)),
            Sale(2, range.EndUtc.AddTicks(-10), SaleStatus.Paid, (1, 1, 300, 200)),
            Sale(1, range.StartUtc.AddHours(1), SaleStatus.Cancelled, (1, 1, 900, 0)),
            Sale(1, range.StartUtc.AddHours(2), SaleStatus.Refunded, (1, 1, 900, 0)),
            Sale(1, range.EndUtc, SaleStatus.Paid, (1, 1, 900, 0)),
            Sale(1, range.StartUtc.AddTicks(-10), SaleStatus.Paid, (1, 1, 50, 30)));
        await db.SaveChangesAsync();
        var result = (await client.GetFromJsonAsync<DashboardDto>($"/api/dashboard?{Query}"))!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Kpis.Revenue, Is.EqualTo("500.00"));
            Assert.That(result.Kpis.GrossProfit, Is.EqualTo("180.00"));
            Assert.That(result.Kpis.SalesCount, Is.EqualTo(2));
            Assert.That(result.Kpis.AverageCheck, Is.EqualTo("250.00"));
            Assert.That(result.Kpis.Margin, Is.EqualTo(.36m));
            Assert.That(result.Kpis.BestManager!.Id, Is.EqualTo(2));
            Assert.That(result.Ranking[0].Manager.IsActive, Is.False);
            Assert.That(result.Series, Has.Length.EqualTo(7));
            Assert.That(result.Series.Sum(d => d.SalesCount), Is.EqualTo(2));
            Assert.That(result.Categories.Sum(c => c.SalesCount), Is.EqualTo(3));
            Assert.That(result.TopProducts[0].Id, Is.EqualTo(1));
            Assert.That(result.Comparison.Metrics.Revenue, Is.EqualTo("50.00"));
        });
        var sales = (await client.GetFromJsonAsync<SalesDto>($"/api/sales?{Query}"))!;
        Assert.That(sales.Items, Has.Length.EqualTo(4));
        Assert.That(sales.Items.Count(s => !s.IncludedInKpis), Is.EqualTo(2));
    }

    [Test]
    public async Task Ranking_switches_ties_are_stable_and_best_is_independent()
    {
        await using var db = Db();
        db.Sales.AddRange(Sale(1, range.StartUtc, SaleStatus.Paid, (1, 1, 100, 0)),
            Sale(2, range.StartUtc, SaleStatus.Paid, (1, 1, 300, 200)),
            Sale(3, range.StartUtc, SaleStatus.Paid, (1, 1, 50, 100)));
        await db.SaveChangesAsync();
        var profit = (await client.GetFromJsonAsync<DashboardDto>($"/api/dashboard?{Query}"))!;
        var check = (await client.GetFromJsonAsync<DashboardDto>($"/api/dashboard?{Query}&rankingBy=averageCheck"))!;
        Assert.That(profit.Ranking.Select(r => r.Manager.Id), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        Assert.That(check.Ranking.Select(r => r.Manager.Id), Is.EqualTo(new[] { 2, 1, 3, 4 }));
        Assert.That(check.Kpis.BestManager!.Id, Is.EqualTo(1));
        Assert.That(profit.Ranking[^1].Rank, Is.Null);
    }

    [Test]
    public async Task Empty_period_and_zero_revenue_are_distinct()
    {
        var empty = (await client.GetFromJsonAsync<DashboardDto>($"/api/dashboard?{Query}"))!;
        Assert.That(empty.Kpis.AverageCheck, Is.Null);
        Assert.That(empty.Kpis.BestManager, Is.Null);
        Assert.That(empty.Ranking.All(r => r.Rank is null), Is.True);
        Assert.That(empty.Categories, Is.Empty);
        await using var db = Db();
        db.Sales.Add(Sale(1, range.StartUtc, SaleStatus.Paid, (1, 1, 0, 100)));
        await db.SaveChangesAsync();
        var zero = (await client.GetFromJsonAsync<DashboardDto>($"/api/dashboard?{Query}"))!;
        Assert.That(zero.Kpis.AverageCheck, Is.EqualTo("0.00"));
        Assert.That(zero.Kpis.Margin, Is.Null);
        Assert.That(zero.Kpis.BestManager!.Id, Is.EqualTo(1));
        Assert.That(zero.Kpis.GrossProfit, Is.EqualTo("-100.00"));
    }

    [TestCase("/api/dashboard?from=bad&to=2026-01-01", "from")]
    [TestCase("/api/dashboard?from=2026-01-01", "to")]
    [TestCase("/api/dashboard?rankingBy=no", "rankingBy")]
    [TestCase("/api/dashboard?preset=no", "preset")]
    [TestCase("/api/sales?limit=101", "limit")]
    [TestCase("/api/sales?limit=0", "limit")]
    [TestCase("/api/sales?limit=bad", "limit")]
    public async Task Invalid_queries_return_problem_details(string path, string key)
    {
        var response = await client.GetAsync(path);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/problem+json"));
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(json.RootElement.GetProperty("errors").TryGetProperty(key, out _), Is.True);
        Assert.That(json.RootElement.TryGetProperty("traceId", out _), Is.True);
    }

    [Test]
    public async Task Full_seed_is_idempotent_and_matches_database_totals()
    {
        await using var db = Db();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE \"SaleItems\", \"Sales\", \"Products\", \"Categories\", \"Customers\", \"Managers\", \"SeedRuns\" RESTART IDENTITY CASCADE");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Seed:AnchorDate"] = "2026-09-25", ["Seed:Enabled"] = "true" }).Build();
        IDatabaseInitializer initializer = new DatabaseInitializer(db, config, TimeProvider.System, NullLogger<DatabaseInitializer>.Instance);
        await initializer.InitializeAsync();
        var count = await db.SaleItems.CountAsync();
        await initializer.InitializeAsync();
        Assert.That(await db.Sales.CountAsync(), Is.EqualTo(3000));
        Assert.That(await db.SaleItems.CountAsync(), Is.EqualTo(count));
        Assert.That(await db.SeedRuns.CountAsync(), Is.EqualTo(1));
        var expected = await db.SaleItems.Where(i => i.Sale.Status == SaleStatus.Paid).SumAsync(i => i.Quantity * i.UnitPrice);
        var response = (await client.GetFromJsonAsync<DashboardDto>("/api/dashboard?from=2025-09-26&to=2026-09-25"))!;
        Assert.That(response.Kpis.Revenue, Is.EqualTo(Money.Format(expected)));
        Assert.That(response.Series.Sum(d => d.SalesCount), Is.EqualTo(response.Kpis.SalesCount));
        Assert.That(response.Ranking, Has.Length.EqualTo(20));

        // Inspect the actual SQL on the full dataset, and guard against N+1 regressions.
        var capture = new QueryCapture();
        await using var measured = new SalesDbContext(new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).AddInterceptors(capture).Options);
        IAnalyticsService analytics = new AnalyticsService(measured);
        await analytics.GetAsync(new DateRange(new DateOnly(2025, 9, 26), new DateOnly(2026, 9, 25)), "grossProfit", default);
        Assert.That(capture.Commands, Has.Count.EqualTo(6));
        await using var connection = new Npgsql.NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        var plans = new List<string>();
        foreach (var command in capture.Commands)
        {
            using (command)
            {
                plans.Add(command.CommandText);
                command.Connection = connection;
                command.CommandText = "EXPLAIN (ANALYZE, BUFFERS) " + command.CommandText;
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) plans.Add(reader.GetString(0));
            }
        }
        var planPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "analytics-query-plans.txt");
        await File.WriteAllLinesAsync(planPath, plans);
        TestContext.AddTestAttachment(planPath, "Actual SQL and PostgreSQL plans on 3000 sales");
    }

    [Test]
    public async Task Database_rejects_invalid_quantity()
    {
        await using var db = Db();
        db.Sales.Add(Sale(1, range.StartUtc, SaleStatus.Paid, (1, 0, 100, 60)));
        Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        if (postgres is not null) await postgres.DisposeAsync();
    }

    private sealed class ApiFactory(string connection) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Sales", connection);
            builder.UseSetting("Seed:Enabled", "false");
            builder.UseEnvironment("Production");
        }
    }
}
