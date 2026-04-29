using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
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
    private readonly IProductService _productService;

    public ProductsModel(ApplicationDbContext context, IProductService productService)
    {
        _context = context;
        _productService = productService;
    }

    public List<ProductViewModel> Products { get; set; } = new();
    public List<string> AvailableWarehouses { get; set; } = new();
    public List<string> AvailableTypes { get; set; } = new();
    public ProductViewModel? SelectedProduct { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedProductId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CurrentPageParam { get; set; }

    [BindProperty]
    public ProductViewModel NewProduct { get; set; } = new();

    [BindProperty]
    public ProductViewModel EditProduct { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public bool ShowAddModal { get; set; } = false;

    // Visibility settings (in a real app, these would come from database/user profile)
    public bool ShowProductCode { get; set; } = true;
    public bool ShowProductName { get; set; } = true;
    public bool ShowWarehouse { get; set; } = true;
    public bool ShowType { get; set; } = true;
    public bool ShowCondition { get; set; } = true;
    public bool ShowGroup { get; set; } = false;
    public bool ShowDate { get; set; } = false;
    public bool ShowCatalogue { get; set; } = false;
    public bool ShowStatus { get; set; } = true;
    public bool ShowBarcode { get; set; } = false;
    public bool ShowExpiryDate { get; set; } = true;

    // Pagination settings
    public int ItemsPerPage { get; set; } = 5;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; } = 0;

    public async Task OnGetAsync()
    {
        if (CurrentPageParam.HasValue)
        {
            CurrentPage = CurrentPageParam.Value;
        }

        // Load settings from Session if available (saved from Settings page)
        var settingsJson = HttpContext.Session.GetString("ProductSettings");
        if (settingsJson != null)
        {
            var settings = JsonSerializer.Deserialize<ProductSettings>(settingsJson);
            if (settings != null)
            {
                ShowProductCode = settings.ShowProductCode;
                ShowProductName = settings.ShowProductName;
                ShowWarehouse = settings.ShowWarehouse;
                ShowType = settings.ShowType;
                ShowCondition = settings.ShowCondition;
                ShowGroup = settings.ShowGroup;
                ShowDate = settings.ShowDate;
                ShowCatalogue = settings.ShowCatalogue;
                ShowStatus = settings.ShowStatus;
                ShowBarcode = settings.ShowBarcode;
                ShowExpiryDate = settings.ShowExpiryDate;
                ItemsPerPage = settings.ItemsPerPage;
            }
        }

        await LoadProductsAsync();
    }

    // Helper class to match the settings structure
    private class ProductSettings
    {
        public int ItemsPerPage { get; set; }
        public bool ShowProductCode { get; set; }
        public bool ShowProductName { get; set; }
        public bool ShowWarehouse { get; set; }
        public bool ShowType { get; set; }
        public bool ShowCondition { get; set; }
        public bool ShowGroup { get; set; }
        public bool ShowDate { get; set; }
        public bool ShowCatalogue { get; set; }
        public bool ShowStatus { get; set; }
        public bool ShowBarcode { get; set; }
        public bool ShowExpiryDate { get; set; }
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var dto = new CreateProductDto
        {
            ProductCode = NewProduct.ProductCode,
            ProductName = NewProduct.ProductName,
            Group = NewProduct.Group,
            Barcode = NewProduct.Barcode,
            Warehouse = NewProduct.Warehouse,
            Catalogue = NewProduct.Catalogue,
            Type = NewProduct.Type,
            Condition = NewProduct.Condition,
            Date = NewProduct.Date == default ? DateTime.UtcNow : NewProduct.Date,
            ExpiryDate = NewProduct.ExpiryDate
        };

        try
        {
            var result = await _productService.CreateProductAsync(dto);
            return RedirectToPage(new { selectedProductId = result.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ShowAddModal = true;
            await LoadProductsAsync();
            return Page();
        }
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
        var allProducts = await _context.Products
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

        TotalItems = allProducts.Count;
        TotalPages = (int)Math.Ceiling((double)TotalItems / ItemsPerPage);

        // Ensure current page is valid
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

        Products = allProducts
            .Skip((CurrentPage - 1) * ItemsPerPage)
            .Take(ItemsPerPage)
            .ToList();

        AvailableWarehouses = allProducts
            .Where(p => !string.IsNullOrWhiteSpace(p.Warehouse))
            .Select(p => p.Warehouse!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        AvailableTypes = allProducts
            .Where(p => !string.IsNullOrWhiteSpace(p.Type))
            .Select(p => p.Type!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        SelectedProduct = SelectedProductId.HasValue
            ? allProducts.FirstOrDefault(p => p.Id == SelectedProductId.Value)
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
