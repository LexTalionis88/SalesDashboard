using Microsoft.EntityFrameworkCore;
using SalesDashboard.DataAccess.Abstractions.Services;
using SalesDashboard.DataAccess.Data;

namespace SalesDashboard.DataAccess.Infrastructure;

internal sealed class DatabaseReadiness(SalesDbContext db) : IDatabaseReadiness
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken)
        => db.Database.CanConnectAsync(cancellationToken);
}
