namespace WarehouseInventory.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = "pcs";
    public bool IsActive { get; set; }
}

public class CreateProductDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = "pcs";
}

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = "pcs";
    public bool IsActive { get; set; }
}
