using System.ComponentModel.DataAnnotations;

namespace WarehouseInventory.Domain.Entities;

public class Warehouse : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
}
