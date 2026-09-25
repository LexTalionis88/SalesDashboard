using Microsoft.EntityFrameworkCore;
using SalesDashboard.DataAccess.Domain;

namespace SalesDashboard.DataAccess.Data;

internal sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SeedRun> SeedRuns => Set<SeedRun>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Manager>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(120);
            b.Property(x => x.Team).HasMaxLength(80);
            b.Property(x => x.Position).HasMaxLength(80);
            b.Property(x => x.Initials).HasMaxLength(8);
            b.Property(x => x.AvatarUrl).HasMaxLength(500);
        });
        model.Entity<Customer>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(120);
            b.Property(x => x.Company).HasMaxLength(160);
            b.Property(x => x.Segment).HasMaxLength(80);
        });
        model.Entity<Category>().Property(x => x.Name).HasMaxLength(100);
        model.Entity<Product>(b =>
        {
            b.Property(x => x.Name).HasMaxLength(160);
            b.Property(x => x.Sku).HasMaxLength(40);
            b.HasIndex(x => x.Sku).IsUnique();
            b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<Sale>(b =>
        {
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.SoldAt, x.Id }).IsDescending();
            b.HasIndex(x => new { x.SoldAt, x.ManagerId }).HasFilter("\"Status\" = 'Paid'");
            b.ToTable(t => t.HasCheckConstraint("CK_Sales_Status", "\"Status\" IN ('Paid', 'Cancelled', 'Refunded')"));
        });
        model.Entity<SaleItem>(b =>
        {
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.UnitCost).HasPrecision(18, 2);
            b.HasOne(x => x.Sale).WithMany(x => x.Items).HasForeignKey(x => x.SaleId);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Items_Quantity", "\"Quantity\" > 0");
                t.HasCheckConstraint("CK_Items_Price", "\"UnitPrice\" >= 0");
                t.HasCheckConstraint("CK_Items_Cost", "\"UnitCost\" >= 0");
            });
        });
    }
}

