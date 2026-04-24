namespace WarehouseInventory.Domain.Entities;

public class ProductStock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; } = 0;
    public DateTime? ExpiryDate { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
    public ICollection<ExpiryWarning> ExpiryWarnings { get; set; } = new List<ExpiryWarning>();
}
