using Microsoft.EntityFrameworkCore;
using SalesDashboard.Api.Abstractions.Services;
using SalesDashboard.Api.Data;
using SalesDashboard.Api.Domain;
using SalesDashboard.Api.Features.Analytics;
using SalesDashboard.Api.Infrastructure;

namespace SalesDashboard.Api.Features.Sales;

public sealed record CustomerDto(int Id, string Name, string Company, string Segment);
public sealed record SaleItemDto(int ProductId, string ProductName, int Quantity, string UnitPrice, string UnitCost);
public sealed record SaleDto(int Id, DateTime SoldAt, ManagerDto Manager, CustomerDto Customer, string Status,
    bool IncludedInKpis, string Amount, string GrossProfit, SaleItemDto[] Items);
public sealed record SalesDto(PeriodDto Period, string Currency, int Limit, SaleDto[] Items);

internal sealed class SalesService(SalesDbContext db) : ISalesService
{
    /// <summary>Возвращает последние продажи с позициями за выбранный период.</summary>
    public async Task<SalesDto> GetAsync(DateRange range, int limit, CancellationToken ct)
    {
        if (limit is < 1 or > 100) throw new RequestValidationException("limit", "Допустимы значения от 1 до 100.");
        var sales = await db.Sales.AsNoTracking().Where(s => s.SoldAt >= range.StartUtc && s.SoldAt < range.EndUtc)
            .OrderByDescending(s => s.SoldAt).ThenByDescending(s => s.Id).Take(limit)
            .Select(s => new
            {
                s.Id, s.SoldAt, s.Status,
                Manager = new ManagerDto(s.Manager.Id, s.Manager.Name, s.Manager.Team, s.Manager.Position, s.Manager.IsActive, s.Manager.Initials, s.Manager.AvatarUrl),
                Customer = new CustomerDto(s.Customer.Id, s.Customer.Name, s.Customer.Company, s.Customer.Segment),
                Amount = s.Items.Sum(i => i.Quantity * i.UnitPrice), Profit = s.Items.Sum(i => i.Quantity * (i.UnitPrice - i.UnitCost)),
                Items = s.Items.OrderBy(i => i.Id).Select(i => new { i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice, i.UnitCost }).ToArray()
            }).ToListAsync(ct);
        return new SalesDto(PeriodDto.FromRange(range), "RUB", limit, sales.Select(s => new SaleDto(s.Id, s.SoldAt, s.Manager, s.Customer,
            s.Status.ToString(), s.Status == SaleStatus.Paid, Money.Format(s.Amount), Money.Format(s.Profit),
            s.Items.Select(i => new SaleItemDto(i.ProductId, i.Name, i.Quantity, Money.Format(i.UnitPrice), Money.Format(i.UnitCost))).ToArray())).ToArray());
    }
}
