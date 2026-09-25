using Microsoft.EntityFrameworkCore;
using SalesDashboard.Application.Infrastructure;
using SalesDashboard.DataAccess.Abstractions.Services;
using SalesDashboard.DataAccess.Data;
using SalesDashboard.DataAccess.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSalesDataAccess(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSalesApplication();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapOpenApi();
app.MapControllers();

if (!EF.IsDesignTime)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
}

app.Run();

public partial class Program;
