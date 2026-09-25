namespace SalesDashboard.DataAccess.Domain;

internal enum SaleStatus { Paid, Cancelled, Refunded }

internal sealed class Manager
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Team { get; set; } = "";
    public string Position { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string Initials { get; set; } = "";
    public string? AvatarUrl { get; set; }
}

internal sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Company { get; set; } = "";
    public string Segment { get; set; } = "";
}

internal sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

internal sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

internal sealed class Sale
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime SoldAt { get; set; }
    public SaleStatus Status { get; set; }
    public List<SaleItem> Items { get; set; } = [];
}

internal sealed class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
}

internal sealed class SeedRun
{
    public int Id { get; set; }
    public int Version { get; set; }
    public int RandomSeed { get; set; }
    public DateOnly AnchorDate { get; set; }
}

