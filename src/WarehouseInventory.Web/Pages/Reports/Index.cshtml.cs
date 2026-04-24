using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.Web.Pages.Reports;

public class ReportsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ReportsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // KPI Data
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int LowStockItems { get; set; }
    public int ExpiringItems { get; set; }

    // Report Type
    public string ReportType { get; set; } = "StockLevels";

    // Filter Options
    public List<string> AvailableWarehouses { get; set; } = new();
    public List<string> AvailableTypes { get; set; } = new();

    // Current Filters
    public string? FilterDateRange { get; set; }
    public string? FilterWarehouse { get; set; }
    public string? FilterType { get; set; }

    // Report Data
    public List<StockLevelReportItem> StockLevels { get; set; } = new();
    public List<LowStockReportItem> LowStock { get; set; } = new();
    public List<ExpiryReportItem> ExpiryItems { get; set; } = new();
    public List<StockMovementReportItem> StockMovements { get; set; } = new();

    public async Task OnGetAsync(string? reportType = null, string? filterDateRange = null, string? filterWarehouse = null, string? filterType = null)
    {
        ReportType = reportType ?? "StockLevels";
        FilterDateRange = filterDateRange;
        FilterWarehouse = filterWarehouse;
        FilterType = filterType;

        var allProducts = await GetProductsAsync();
        var allStock = await GetStockAsync();
        var warehouses = await GetWarehousesAsync();

        var products = ApplyProductFilters(allProducts);
        var stock = ApplyStockFilters(allStock, products);
        var filteredWarehouses = ApplyWarehouseFilters(warehouses);

        // Calculate KPIs
        TotalProducts = allProducts.Count;
        TotalStock = allStock.Sum(s => s.Quantity);
        LowStockItems = allStock.Count(s => s.Quantity < 10);
        ExpiringItems = allProducts.Count(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= DateTime.UtcNow.AddDays(30));

        // Available filter options
        AvailableWarehouses = warehouses.Select(w => w.Name).Distinct().OrderBy(w => w).ToList();
        AvailableTypes = allProducts.Select(p => p.Type).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t).Cast<string>().ToList();

        // Generate report data based on type
        switch (ReportType)
        {
            case "StockLevels":
                StockLevels = GenerateStockLevelsReport(stock, filteredWarehouses);
                break;
            case "LowStock":
                LowStock = GenerateLowStockReport(stock);
                break;
            case "ExpiryDates":
                ExpiryItems = GenerateExpiryReport(products);
                break;
            case "StockMovements":
                StockMovements = await GenerateStockMovementsReportAsync(products);
                break;
        }
    }

    private List<ProductItem> ApplyProductFilters(List<ProductItem> products)
    {
        if (!string.IsNullOrEmpty(FilterWarehouse))
        {
            products = products.Where(p => p.Warehouse == FilterWarehouse).ToList();
        }

        if (!string.IsNullOrEmpty(FilterType))
        {
            products = products.Where(p => p.Type == FilterType).ToList();
        }

        return products;
    }

    private List<StockItem> ApplyStockFilters(List<StockItem> stock, List<ProductItem> filteredProducts)
    {
        if (!string.IsNullOrEmpty(FilterWarehouse))
        {
            stock = stock.Where(s => s.Warehouse == FilterWarehouse).ToList();
        }

        if (!string.IsNullOrEmpty(FilterType))
        {
            var productCodes = filteredProducts.Select(p => p.ProductCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            stock = stock.Where(s => productCodes.Contains(s.ProductCode)).ToList();
        }

        return stock;
    }

    private List<WarehouseItem> ApplyWarehouseFilters(List<WarehouseItem> warehouses)
    {
        if (!string.IsNullOrEmpty(FilterWarehouse))
        {
            warehouses = warehouses.Where(w => w.Name == FilterWarehouse).ToList();
        }

        return warehouses;
    }

    private List<StockLevelReportItem> GenerateStockLevelsReport(List<StockItem> stock, List<WarehouseItem> warehouses)
    {
        var report = new List<StockLevelReportItem>();
        foreach (var warehouse in warehouses)
        {
            var warehouseStock = stock.Where(s => s.Warehouse == warehouse.Name).ToList();
            report.Add(new StockLevelReportItem
            {
                Warehouse = warehouse.Name,
                TotalProducts = warehouseStock.Count,
                TotalQuantity = warehouseStock.Sum(s => s.Quantity),
                AverageQuantity = warehouseStock.Any() ? (int)warehouseStock.Average(s => s.Quantity) : 0
            });
        }
        return report;
    }

    private List<LowStockReportItem> GenerateLowStockReport(List<StockItem> stock)
    {
        return stock.Where(s => s.Quantity < 10)
            .Select(s => new LowStockReportItem
            {
                ProductCode = s.ProductCode,
                ProductName = s.ProductName,
                Warehouse = s.Warehouse,
                CurrentQuantity = s.Quantity,
                Status = s.Quantity == 0 ? "Out of Stock" : "Low Stock"
            })
            .OrderBy(s => s.CurrentQuantity)
            .ToList();
    }

    private List<ExpiryReportItem> GenerateExpiryReport(List<ProductItem> products)
    {
        var expiryItems = products.Where(p => p.ExpiryDate.HasValue)
            .Select(p => new ExpiryReportItem
            {
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Warehouse = p.Warehouse ?? "N/A",
                ExpiryDate = p.ExpiryDate.Value,
                DaysUntilExpiry = (int)(p.ExpiryDate.Value - DateTime.UtcNow).TotalDays
            })
            .Where(p => p.DaysUntilExpiry <= GetMaxDaysFromDateRange())
            .OrderBy(p => p.DaysUntilExpiry)
            .ToList();

        return expiryItems;
    }

    private async Task<List<StockMovementReportItem>> GenerateStockMovementsReportAsync(List<ProductItem> filteredProducts)
    {
        var movements = await _context.StockTransactions
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .OrderByDescending(t => t.PerformedAt)
            .Select(t => new StockMovementReportItem
            {
                Date = t.PerformedAt,
                ProductCode = t.Product.ProductCode,
                ProductName = t.Product.Name,
                Warehouse = t.Warehouse.Name,
                Type = t.TransactionType == Domain.Entities.TransactionType.StockIn ? "Stock In" : t.TransactionType == Domain.Entities.TransactionType.StockOut ? "Stock Out" : t.TransactionType.ToString(),
                Quantity = (int)Math.Round(t.Quantity)
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(FilterWarehouse))
        {
            movements = movements.Where(m => m.Warehouse == FilterWarehouse).ToList();
        }

        if (!string.IsNullOrEmpty(FilterType))
        {
            var productCodes = filteredProducts.Select(p => p.ProductCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            movements = movements.Where(m => productCodes.Contains(m.ProductCode)).ToList();
        }

        var maxDays = GetMaxDaysFromDateRange();
        movements = movements.Where(m => (DateTime.UtcNow - m.Date).TotalDays <= maxDays).OrderByDescending(m => m.Date).ToList();

        return movements;
    }

    private int GetMaxDaysFromDateRange()
    {
        return FilterDateRange switch
        {
            "7" => 7,
            "30" => 30,
            "90" => 90,
            _ => 3650
        };
    }

    private async Task<List<ProductItem>> GetProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Warehouse)
            .Select(p => new ProductItem
            {
                ProductCode = p.ProductCode,
                ProductName = p.Name,
                Warehouse = p.Warehouse != null ? p.Warehouse.Name : null,
                Type = p.Type,
                ExpiryDate = p.ExpiryDate
            })
            .ToListAsync();
    }

    private async Task<List<StockItem>> GetStockAsync()
    {
        return await _context.ProductStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Select(s => new StockItem
            {
                ProductCode = s.Product.ProductCode,
                ProductName = s.Product.Name,
                Warehouse = s.Warehouse.Name,
                Quantity = (int)Math.Round(s.Quantity)
            })
            .ToListAsync();
    }

    private async Task<List<WarehouseItem>> GetWarehousesAsync()
    {
        return await _context.Warehouses
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseItem
            {
                Name = w.Name
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostExportAsync(string reportType, string? filterDateRange = null, string? filterWarehouse = null, string? filterType = null)
    {
        // Use the passed report type, fallback to current model value
        var currentReportType = !string.IsNullOrEmpty(reportType) ? reportType : ReportType;
        
        // Temporarily set filter values for this export
        var originalFilterWarehouse = FilterWarehouse;
        var originalFilterType = FilterType;
        var originalFilterDateRange = FilterDateRange;
        
        FilterWarehouse = filterWarehouse;
        FilterType = filterType;
        FilterDateRange = filterDateRange;

        var allProducts = await GetProductsAsync();
        var allStock = await GetStockAsync();
        var warehouses = await GetWarehousesAsync();

        var products = ApplyProductFilters(allProducts);
        var stock = ApplyStockFilters(allStock, products);
        var filteredWarehouses = ApplyWarehouseFilters(warehouses);

        var csv = new StringBuilder();
        
        switch (currentReportType)
        {
            case "StockLevels":
                csv.AppendLine("Warehouse,Total Products,Total Quantity,Average Quantity");
                var stockLevels = GenerateStockLevelsReport(stock, filteredWarehouses);
                foreach (var item in stockLevels)
                {
                    csv.AppendLine($"\"{item.Warehouse}\",{item.TotalProducts},{item.TotalQuantity},{item.AverageQuantity}");
                }
                break;
            case "LowStock":
                csv.AppendLine("Product Code,Product Name,Warehouse,Current Quantity,Status");
                var lowStock = GenerateLowStockReport(stock);
                foreach (var item in lowStock)
                {
                    csv.AppendLine($"\"{item.ProductCode}\",\"{item.ProductName}\",\"{item.Warehouse}\",{item.CurrentQuantity},\"{item.Status}\"");
                }
                break;
            case "ExpiryDates":
                csv.AppendLine("Product Code,Product Name,Warehouse,Expiry Date,Days Until Expiry");
                var expiryItems = GenerateExpiryReport(products);
                foreach (var item in expiryItems)
                {
                    csv.AppendLine($"\"{item.ProductCode}\",\"{item.ProductName}\",\"{item.Warehouse}\",\"{item.ExpiryDate:yyyy-MM-dd}\",{item.DaysUntilExpiry}");
                }
                break;
            case "StockMovements":
                csv.AppendLine("Date,Product Code,Product Name,Warehouse,Type,Quantity");
                var stockMovements = await GenerateStockMovementsReportAsync(products);
                foreach (var item in stockMovements)
                {
                    csv.AppendLine($"\"{item.Date:yyyy-MM-dd}\",\"{item.ProductCode}\",\"{item.ProductName}\",\"{item.Warehouse}\",\"{item.Type}\",{item.Quantity}");
                }
                break;
        }

        // Restore original filter values
        FilterWarehouse = originalFilterWarehouse;
        FilterType = originalFilterType;
        FilterDateRange = originalFilterDateRange;

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"{currentReportType}_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

// Report Item Classes
public class StockLevelReportItem
{
    public string Warehouse { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalQuantity { get; set; }
    public int AverageQuantity { get; set; }
}

public class LowStockReportItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ExpiryReportItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
}

public class StockMovementReportItem
{
    public DateTime Date { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

// Helper data classes
public class ProductItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Warehouse { get; set; }
    public string? Type { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class StockItem
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class WarehouseItem
{
    public string Name { get; set; } = string.Empty;
}
