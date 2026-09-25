using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Abstractions.Services;
using SalesDashboard.Api.Data;
using SalesDashboard.Api.Data.Seed;
using SalesDashboard.Api.Features.Analytics;
using SalesDashboard.Api.Features.Sales;
using SalesDashboard.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Sales") ?? throw new InvalidOperationException("ConnectionStrings:Sales is required.")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapOpenApi();
app.MapGet("/api/dashboard", async ([FromQuery] string? from, [FromQuery] string? to, [FromQuery] string? preset,
    [FromQuery] string? rankingBy, IAnalyticsService analytics, TimeProvider clock, CancellationToken ct) =>
    await analytics.GetAsync(DateRange.Parse(from, to, preset, clock), rankingBy ?? "grossProfit", ct))
    .WithName("GetDashboard").ProducesValidationProblem().ProducesProblem(500);
app.MapGet("/api/sales", async ([FromQuery] string? from, [FromQuery] string? to, [FromQuery] string? preset,
    [FromQuery] string? limit, ISalesService sales, TimeProvider clock, CancellationToken ct) =>
{
    var parsedLimit = 20;
    if (limit is not null && !int.TryParse(limit, NumberStyles.None, CultureInfo.InvariantCulture, out parsedLimit))
        throw new RequestValidationException("limit", "Требуется целое число от 1 до 100.");
    return await sales.GetAsync(DateRange.Parse(from, to, preset, clock), parsedLimit, ct);
}).WithName("GetSales").ProducesValidationProblem().ProducesProblem(500);
app.MapGet("/api/health/ready", async (SalesDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.Problem(statusCode: 503, title: "Database unavailable"));
if (!EF.IsDesignTime)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
}
app.Run();
public partial class Program;
