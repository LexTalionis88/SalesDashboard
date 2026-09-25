using SalesDashboard.DataAccess.Domain;
using SalesDashboard.DataAccess.Infrastructure;

namespace SalesDashboard.DataAccess.Data.Seed;

internal sealed record SeedData(
    Manager[] Managers,
    Customer[] Customers,
    Category[] Categories,
    Product[] Products,
    Sale[] Sales);

internal static class SeedGenerator
{
    public const int Version = 1;
    public const int DefaultSeed = 20260925;
    public static SeedData Generate(DateOnly anchor, int seed)
    {
        var random = new StableRandom(seed);
        string[] first =
            ["Анна", "Михаил", "Елена", "Дмитрий", "Ольга", "Алексей", "Мария", "Сергей", "Ирина", "Павел"];
        string[] last =
            ["Соколова", "Петров", "Орлова", "Волков", "Морозова", "Смирнов", "Крылова", "Попов",
                "Кузнецова", "Лебедев"];
        var managers = Enumerable.Range(1, 20).Select(i => new Manager
        {
            Id = i,
            Name = $"{first[(i - 1) % 10]} {last[(i - 1) % 10]}{(i > 10 ? " II" : "")}",
            Team = i <= 10 ? "Корпоративные продажи" : "Региональные продажи",
            Position = i <= 3 ? "Ведущий менеджер" : "Менеджер",
            Initials = $"{first[(i - 1) % 10][0]}{last[(i - 1) % 10][0]}",
            IsActive = i != 18
        }).ToArray();
        var customers = Enumerable.Range(1, 75).Select(i => new Customer
        {
            Id = i,
            Name = $"Контакт {i:00}",
            Company = $"Компания {i:00}",
            Segment = new[] { "SMB", "Enterprise", "Retail" }[i % 3]
        }).ToArray();
        string[] categoryNames = ["Дроны", "Камеры", "Стабилизаторы", "Объективы", "Аксессуары", "Аудио"];
        var categories = categoryNames.Select((name, i) => new Category { Id = i + 1, Name = name }).ToArray();
        var products = Enumerable.Range(1, 48).Select(i => new Product
        {
            Id = i,
            Name = $"{categoryNames[(i - 1) / 8]} — модель {(i - 1) % 8 + 1}",
            Sku = $"SKU-{i:000}",
            CategoryId = (i - 1) / 8 + 1
        }).ToArray();
        var firstDay = anchor.AddMonths(-12).AddDays(1);
        var totalDays = anchor.DayNumber - firstDay.DayNumber + 1;
        var sales = new List<Sale>(3000);
        var itemId = 1;
        for (var id = 1; id <= 3000; id++)
        {
            DateOnly date;
            do { date = firstDay.AddDays(random.Next(totalDays)); }
            while (date.Month is not (11 or 12) && random.Next(100) < 40);
            var managerId = Math.Min(random.Next(18), random.Next(18)) + 1;
            if (managerId == 18 && date > anchor.AddDays(-45)) date = anchor.AddDays(-60 - random.Next(100));
            var statusDraw = random.Next(100);
            var sale = new Sale
            {
                Id = id,
                ManagerId = managerId,
                CustomerId = random.Next(75) + 1,
                SoldAt = DateRange.ToUtc(date).AddHours(8 + random.Next(12)).AddMinutes(random.Next(60)),
                Status = statusDraw < 82
                    ? SaleStatus.Paid
                    : statusDraw < 93
                        ? SaleStatus.Cancelled
                        : SaleStatus.Refunded
            };
            var itemCount = 1 + random.Next(5);
            for (var j = 0; j < itemCount; j++)
            {
                var product = random.Next(48) + 1;
                var price = decimal.Round((1000 + product * 1100m) * (80 + random.Next(50)) / 100m, 2);
                sale.Items.Add(new SaleItem
                {
                    Id = itemId++,
                    ProductId = product,
                    Quantity = 1 + random.Next(managerId <= 4 ? 12 : 4),
                    UnitPrice = price,
                    UnitCost = decimal.Round(price * (55 + random.Next(55)) / 100m, 2)
                });
            }
            sales.Add(sale);
        }
        sales[0].SoldAt = DateRange.ToUtc(firstDay);
        sales[0].ManagerId = 20;
        for (var i = 1; i <= 2; i++)
        {
            sales[i].SoldAt = DateRange.ToUtc(anchor);
            sales[i].ManagerId = i == 1 ? 19 : 20;
            sales[i].Status = SaleStatus.Paid;
            sales[i].Items =
                [new SaleItem { Id = itemId++, ProductId = 1, Quantity = 1, UnitPrice = 10000, UnitCost = 6000 }];
        }
        sales[3].Status = SaleStatus.Paid;
        sales[3].Items =
            [new SaleItem { Id = itemId++, ProductId = 2, Quantity = 100, UnitPrice = 100000, UnitCost = 70000 }];
        for (var i = 4; i < 104; i++)
        {
            sales[i].Status = SaleStatus.Paid;
            sales[i].Items =
                [new SaleItem { Id = itemId++, ProductId = 40, Quantity = 1, UnitPrice = 100, UnitCost = 80 }];
        }
        sales[104].Status = SaleStatus.Paid;
        sales[104].Items = [new SaleItem { Id = itemId++, ProductId = 3, Quantity = 1, UnitPrice = 0, UnitCost = 100 }];
        sales[105].Status = SaleStatus.Cancelled;
        sales[106].Status = SaleStatus.Refunded;
        return new SeedData(managers, customers, categories, products, sales.ToArray());
    }

    private sealed class StableRandom(int seed)
    {
        private uint state = unchecked((uint)seed) | 1u;
        public int Next(int max)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)max);
        }
    }
}

