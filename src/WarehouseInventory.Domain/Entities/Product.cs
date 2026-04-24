namespace WarehouseInventory.Domain.Entities;

public class Product : BaseEntity
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Group { get; set; }
    public string? Barcode { get; set; }
    public string? Catalogue { get; set; }
    public string? Type { get; set; }
    public string? Condition { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public Guid? WarehouseId { get; set; }
    public string UnitOfMeasure { get; set; } = "pcs";
    public bool IsActive { get; set; } = true;
    public Warehouse? Warehouse { get; set; }
}
