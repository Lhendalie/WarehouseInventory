using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.Web.Pages.Products;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Barcode { get; set; }
    public string? Catalogue { get; set; }
    public string? Warehouse { get; set; }
    public string? Type { get; set; }
    public string? Condition { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class ProductsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ProductsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ProductViewModel> Products { get; set; } = new();
    public List<string> AvailableWarehouses { get; set; } = new();
    public List<string> AvailableTypes { get; set; } = new();
    public ProductViewModel? SelectedProduct { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedProductId { get; set; }

    [BindProperty]
    public ProductViewModel NewProduct { get; set; } = new();

    [BindProperty]
    public ProductViewModel EditProduct { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadProductsAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProductsAsync();
            return Page();
        }

        var warehouseId = await ResolveWarehouseIdAsync(NewProduct.Warehouse);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = NewProduct.ProductCode,
            Name = NewProduct.ProductName,
            Category = NewProduct.Group,
            Group = NewProduct.Group,
            Barcode = NewProduct.Barcode,
            Catalogue = NewProduct.Catalogue,
            WarehouseId = warehouseId,
            Type = NewProduct.Type,
            Condition = NewProduct.Condition,
            Date = NewProduct.Date == default ? DateTime.UtcNow : NewProduct.Date,
            ExpiryDate = NewProduct.ExpiryDate,
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { selectedProductId = product.Id });
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == EditProduct.Id);
        if (product == null)
        {
            await LoadProductsAsync();
            return Page();
        }

        product.ProductCode = EditProduct.ProductCode;
        product.Name = EditProduct.ProductName;
        product.Category = EditProduct.Group;
        product.Group = EditProduct.Group;
        product.Barcode = EditProduct.Barcode;
        product.Catalogue = EditProduct.Catalogue;
        product.WarehouseId = await ResolveWarehouseIdAsync(EditProduct.Warehouse);
        product.Type = EditProduct.Type;
        product.Condition = EditProduct.Condition;
        product.Date = EditProduct.Date;
        product.ExpiryDate = EditProduct.ExpiryDate;
        product.IsActive = EditProduct.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = Guid.Empty;

        await _context.SaveChangesAsync();

        return RedirectToPage(new { selectedProductId = EditProduct.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            var relatedStocks = await _context.ProductStocks.Where(s => s.ProductId == id).ToListAsync();
            var relatedTransactions = await _context.StockTransactions.Where(t => t.ProductId == id).ToListAsync();
            _context.StockTransactions.RemoveRange(relatedTransactions);
            _context.ProductStocks.RemoveRange(relatedStocks);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostRefresh()
    {
        return RedirectToPage(new { selectedProductId = SelectedProductId });
    }

    private async Task LoadProductsAsync()
    {
        Products = await _context.Products
            .Include(p => p.Warehouse)
            .OrderBy(p => p.ProductCode)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductName = p.Name,
                Group = p.Group,
                Barcode = p.Barcode,
                Catalogue = p.Catalogue,
                Warehouse = p.Warehouse != null ? p.Warehouse.Name : null,
                Type = p.Type,
                Condition = p.Condition,
                Date = p.Date,
                ExpiryDate = p.ExpiryDate,
                IsActive = p.IsActive
            })
            .ToListAsync();

        AvailableWarehouses = Products
            .Where(p => !string.IsNullOrWhiteSpace(p.Warehouse))
            .Select(p => p.Warehouse!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        AvailableTypes = Products
            .Where(p => !string.IsNullOrWhiteSpace(p.Type))
            .Select(p => p.Type!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        SelectedProduct = SelectedProductId.HasValue
            ? Products.FirstOrDefault(p => p.Id == SelectedProductId.Value)
            : null;

        if (SelectedProduct != null)
        {
            EditProduct = CloneProduct(SelectedProduct);
        }
    }

    private async Task<Guid?> ResolveWarehouseIdAsync(string? warehouseName)
    {
        if (string.IsNullOrWhiteSpace(warehouseName))
        {
            return null;
        }

        var existingWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseName);
        if (existingWarehouse != null)
        {
            return existingWarehouse.Id;
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = warehouseName,
            Location = warehouseName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse.Id;
    }

    private static ProductViewModel CloneProduct(ProductViewModel product)
    {
        return new ProductViewModel
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            Group = product.Group,
            Barcode = product.Barcode,
            Catalogue = product.Catalogue,
            Warehouse = product.Warehouse,
            Type = product.Type,
            Condition = product.Condition,
            Date = product.Date,
            ExpiryDate = product.ExpiryDate,
            IsActive = product.IsActive
        };
    }
}
