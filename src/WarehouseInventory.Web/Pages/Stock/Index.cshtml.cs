using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.Web.Pages.Stock;

public class StockViewModel
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class StockModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public StockModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<StockViewModel> Stock { get; set; } = new();
    public List<string> AvailableWarehouses { get; set; } = new();

    public string? FilterWarehouse { get; set; }
    public string? SearchTerm { get; set; }

    public async Task OnGetAsync(string? filterWarehouse = null, string? searchTerm = null)
    {
        FilterWarehouse = filterWarehouse;
        SearchTerm = searchTerm;

        Stock = await _context.ProductStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .OrderBy(s => s.Product.ProductCode)
            .ThenBy(s => s.Warehouse.Name)
            .Select(s => new StockViewModel
            {
                Id = s.Id,
                ProductCode = s.Product.ProductCode,
                ProductName = s.Product.Name,
                Warehouse = s.Warehouse.Name,
                Quantity = (int)Math.Round(s.Quantity)
            })
            .ToListAsync();

        AvailableWarehouses = await _context.Warehouses
            .OrderBy(w => w.Name)
            .Select(w => w.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostReceiveAsync(Guid id, int quantity = 1, string? filterWarehouse = null, string? searchTerm = null)
    {
        await ProcessStockChangeAsync(id, quantity, true);

        return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
    }

    public async Task<IActionResult> OnPostDispatchAsync(Guid id, int quantity = 1, string? filterWarehouse = null, string? searchTerm = null)
    {
        await ProcessStockChangeAsync(id, quantity, false);

        return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
    }

    public async Task<IActionResult> OnPostStockInAsync(Guid id, int quantity, string? filterWarehouse = null, string? searchTerm = null)
    {
        if (quantity < 1)
        {
            ModelState.AddModelError("", "Quantity must be at least 1.");
            return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
        }

        await ProcessStockChangeAsync(id, quantity, true);

        return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
    }

    public async Task<IActionResult> OnPostStockOutAsync(Guid id, int quantity, string? filterWarehouse = null, string? searchTerm = null)
    {
        if (quantity < 1)
        {
            ModelState.AddModelError("", "Quantity must be at least 1.");
            return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
        }

        await ProcessStockChangeAsync(id, quantity, false);

        return RedirectToPage("./Index", new { filterWarehouse, searchTerm });
    }

    private async Task ProcessStockChangeAsync(Guid stockId, int quantity, bool isStockIn)
    {
        var stockItem = await _context.ProductStocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == stockId);

        if (stockItem == null)
        {
            return;
        }

        var previousQuantity = stockItem.Quantity;
        var newQuantity = isStockIn ? previousQuantity + quantity : previousQuantity - quantity;

        if (!isStockIn && newQuantity < 0)
        {
            ModelState.AddModelError("", "Cannot dispatch more than available stock.");
            return;
        }

        stockItem.Quantity = newQuantity;
        stockItem.LastUpdated = DateTime.UtcNow;
        stockItem.UpdatedAt = DateTime.UtcNow;
        stockItem.UpdatedBy = Guid.Empty;

        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = stockItem.ProductId,
            WarehouseId = stockItem.WarehouseId,
            TransactionType = isStockIn ? TransactionType.StockIn : TransactionType.StockOut,
            Quantity = quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            Reason = isStockIn ? "Stock In from UI" : "Stock Out from UI",
            PerformedBy = Guid.Empty,
            PerformedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        });

        await _context.SaveChangesAsync();
    }
}
