using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Domain.Entities;

namespace WarehouseInventory.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Warehouses.AnyAsync() || await context.Products.AnyAsync() || await context.ProductStocks.AnyAsync())
        {
            return;
        }

        var systemUserId = Guid.Empty;
        var now = DateTime.UtcNow;

        var mainWarehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = "Main Warehouse",
            Location = "123 Industrial Park, City",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var eastWarehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = "East Distribution Center",
            Location = "456 Commerce Blvd, Eastside",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var westWarehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = "West Storage Facility",
            Location = "789 Logistics Way, Westend",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };

        context.Warehouses.AddRange(mainWarehouse, eastWarehouse, westWarehouse);

        var sampleProduct1 = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = "PROD001",
            Name = "Sample Product 1",
            Category = "Electronics",
            Group = "Electronics",
            Barcode = "1234567890123",
            Catalogue = "CAT-001",
            WarehouseId = mainWarehouse.Id,
            Type = "Finished Goods",
            Condition = "New",
            Date = now.AddDays(-30),
            ExpiryDate = null,
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var sampleProduct2 = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = "PROD002",
            Name = "Sample Product 2",
            Category = "Hardware",
            Group = "Hardware",
            Barcode = "1234567890124",
            Catalogue = "CAT-002",
            WarehouseId = mainWarehouse.Id,
            Type = "Raw Materials",
            Condition = "New",
            Date = now.AddDays(-15),
            ExpiryDate = null,
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var freshBread = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = "FOOD001",
            Name = "Fresh Bread",
            Category = "Food",
            Group = "Food",
            Barcode = "1234567890125",
            Catalogue = "CAT-003",
            WarehouseId = mainWarehouse.Id,
            Type = "Food",
            Condition = "New",
            Date = now,
            ExpiryDate = now.AddDays(1),
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var freshMilk = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = "FOOD002",
            Name = "Fresh Milk",
            Category = "Food",
            Group = "Food",
            Barcode = "1234567890126",
            Catalogue = "CAT-004",
            WarehouseId = mainWarehouse.Id,
            Type = "Food",
            Condition = "New",
            Date = now,
            ExpiryDate = now.AddDays(1),
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };
        var toolY = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = "TOOL001",
            Name = "Tool Y",
            Category = "Tools",
            Group = "Tools",
            Barcode = "1234567890127",
            Catalogue = "CAT-005",
            WarehouseId = westWarehouse.Id,
            Type = "Tools",
            Condition = "New",
            Date = now.AddDays(-7),
            ExpiryDate = null,
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = systemUserId
        };

        context.Products.AddRange(sampleProduct1, sampleProduct2, freshBread, freshMilk, toolY);

        var stocks = new[]
        {
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct1.Id,
                WarehouseId = mainWarehouse.Id,
                Quantity = 150,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct2.Id,
                WarehouseId = mainWarehouse.Id,
                Quantity = 75,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = freshBread.Id,
                WarehouseId = mainWarehouse.Id,
                Quantity = 0,
                ExpiryDate = freshBread.ExpiryDate,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct1.Id,
                WarehouseId = eastWarehouse.Id,
                Quantity = 200,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct2.Id,
                WarehouseId = eastWarehouse.Id,
                Quantity = 50,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct1.Id,
                WarehouseId = westWarehouse.Id,
                Quantity = 100,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = toolY.Id,
                WarehouseId = westWarehouse.Id,
                Quantity = 25,
                LastUpdated = now,
                CreatedAt = now,
                CreatedBy = systemUserId
            }
        };

        context.ProductStocks.AddRange(stocks);

        context.StockTransactions.AddRange(
            new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct1.Id,
                WarehouseId = mainWarehouse.Id,
                TransactionType = TransactionType.StockIn,
                Quantity = 150,
                PreviousQuantity = 0,
                NewQuantity = 150,
                Reason = "Initial seed",
                PerformedBy = systemUserId,
                PerformedAt = now.AddDays(-5),
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct2.Id,
                WarehouseId = mainWarehouse.Id,
                TransactionType = TransactionType.StockIn,
                Quantity = 75,
                PreviousQuantity = 0,
                NewQuantity = 75,
                Reason = "Initial seed",
                PerformedBy = systemUserId,
                PerformedAt = now.AddDays(-4),
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = freshBread.Id,
                WarehouseId = mainWarehouse.Id,
                TransactionType = TransactionType.StockOut,
                Quantity = 5,
                PreviousQuantity = 5,
                NewQuantity = 0,
                Reason = "Initial seed",
                PerformedBy = systemUserId,
                PerformedAt = now.AddDays(-3),
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = sampleProduct1.Id,
                WarehouseId = westWarehouse.Id,
                TransactionType = TransactionType.StockOut,
                Quantity = 30,
                PreviousQuantity = 130,
                NewQuantity = 100,
                Reason = "Initial seed",
                PerformedBy = systemUserId,
                PerformedAt = now.AddDays(-2),
                CreatedAt = now,
                CreatedBy = systemUserId
            },
            new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = toolY.Id,
                WarehouseId = westWarehouse.Id,
                TransactionType = TransactionType.StockIn,
                Quantity = 25,
                PreviousQuantity = 0,
                NewQuantity = 25,
                Reason = "Initial seed",
                PerformedBy = systemUserId,
                PerformedAt = now.AddDays(-1),
                CreatedAt = now,
                CreatedBy = systemUserId
            });

        await context.SaveChangesAsync();
    }
}
