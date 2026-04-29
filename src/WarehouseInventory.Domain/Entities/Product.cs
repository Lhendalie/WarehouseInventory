using System.ComponentModel.DataAnnotations;

namespace WarehouseInventory.Domain.Entities;

public class Product : BaseEntity
{
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? Group { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [MaxLength(50)]
    public string? Catalogue { get; set; }

    [MaxLength(50)]
    public string? Type { get; set; }

    [MaxLength(50)]
    public string? Condition { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public Guid? WarehouseId { get; set; }

    [MaxLength(20)]
    public string UnitOfMeasure { get; set; } = "pcs";

    public bool IsActive { get; set; } = true;

    public Warehouse? Warehouse { get; set; }
}
