namespace WarehouseInventory.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}

public enum TransactionType
{
    StockIn,
    StockOut,
    Transfer,
    Adjustment,
    Hold,
    QualityCheck
}
