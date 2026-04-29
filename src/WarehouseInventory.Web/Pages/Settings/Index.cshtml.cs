using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Domain.Interfaces;

namespace WarehouseInventory.Web.Pages.Settings;

public class SettingsModel : PageModel
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SettingsModel(IRepository<Warehouse> warehouseRepository, IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    [BindProperty]
    public UserSettingsViewModel UserSettings { get; set; } = new();

    [BindProperty]
    public ProductSettingsViewModel ProductSettings { get; set; } = new();

    [BindProperty]
    public ReportSettingsViewModel ReportSettings { get; set; } = new();

    public List<Warehouse> Warehouses { get; set; } = new();

    public int BulkProductCount { get; set; } = 0;

    public BulkCreateResult? BulkCreateResult { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActiveTab { get; set; } = "user";

    public void OnGet()
    {
        // Load warehouses for the dropdown
        Warehouses = _warehouseRepository.GetAllAsync().GetAwaiter().GetResult().ToList();

        // Load bulk creation result from TempData if available
        if (TempData.ContainsKey("BulkCreateResult"))
        {
            BulkCreateResult = JsonSerializer.Deserialize<BulkCreateResult>(TempData["BulkCreateResult"]!.ToString()!);
        }

        // Load from Session if available (persists across the application)
        if (HttpContext.Session.GetString("UserSettings") != null)
        {
            UserSettings = JsonSerializer.Deserialize<UserSettingsViewModel>(HttpContext.Session.GetString("UserSettings")!);
        }
        else
        {
            UserSettings = new UserSettingsViewModel
            {
                DisplayName = User.Identity?.Name ?? "User",
                Email = "user@example.com",
                TimeZone = "UTC",
                Language = "en",
                EnableEmailNotifications = true,
                SessionTimeout = 30,
                RequireTwoFactor = false
            };
        }

        if (HttpContext.Session.GetString("ProductSettings") != null)
        {
            ProductSettings = JsonSerializer.Deserialize<ProductSettingsViewModel>(HttpContext.Session.GetString("ProductSettings")!);
        }
        else
        {
            ProductSettings = new ProductSettingsViewModel
            {
                ItemsPerPage = 5,
                ShowProductCode = true,
                ShowProductName = true,
                ShowWarehouse = true,
                ShowType = true,
                ShowCondition = true,
                ShowGroup = false,
                ShowDate = false,
                ShowCatalogue = false,
                ShowStatus = true,
                ShowBarcode = false,
                ShowExpiryDate = true
            };
        }

        if (HttpContext.Session.GetString("ReportSettings") != null)
        {
            ReportSettings = JsonSerializer.Deserialize<ReportSettingsViewModel>(HttpContext.Session.GetString("ReportSettings")!);
        }
        else
        {
            ReportSettings = new ReportSettingsViewModel
            {
                DefaultDateRange = "30",
                DefaultReportType = "StockLevels",
                IncludeZeroStock = true,
                AutoRefreshReports = false,
                ExportFormat = "CSV",
                WarningDaysThreshold = 30,
                EnableExpiryEmails = true
            };
        }
    }

    public IActionResult OnPostSaveUserSettings()
    {
        // In a real app, save these to the database or user profile
        HttpContext.Session.SetString("UserSettings", JsonSerializer.Serialize(UserSettings));
        TempData["Message"] = "User settings saved successfully.";
        return RedirectToPage(new { activeTab = "user" });
    }

    public IActionResult OnPostSaveProductSettings()
    {
        // In a real app, save these to the database or user profile
        HttpContext.Session.SetString("ProductSettings", JsonSerializer.Serialize(ProductSettings));
        TempData["Message"] = "Product settings saved successfully.";
        return RedirectToPage(new { activeTab = "products" });
    }

    public IActionResult OnPostSaveReportSettings()
    {
        // In a real app, save these to the database or user profile
        HttpContext.Session.SetString("ReportSettings", JsonSerializer.Serialize(ReportSettings));
        TempData["Message"] = "Report settings saved successfully.";
        return RedirectToPage(new { activeTab = "reports" });
    }

    public async Task<IActionResult> OnPostBulkCreateProducts(
        string BulkWarehouseId,
        string BulkProductType,
        string BulkCondition,
        string BulkGroup,
        string BulkProducts,
        bool BulkAutoGenerateBarcodes)
    {
        var result = new BulkCreateResult();

        try
        {
            if (string.IsNullOrWhiteSpace(BulkWarehouseId) || !Guid.TryParse(BulkWarehouseId, out var warehouseId))
            {
                result.ErrorMessage = "Please select a valid warehouse.";
                BulkCreateResult = result;
                return Page();
            }

            if (string.IsNullOrWhiteSpace(BulkProducts))
            {
                result.ErrorMessage = "Please enter at least one product.";
                BulkCreateResult = result;
                return Page();
            }

            var lines = BulkProducts.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var invalidLines = new List<string>();
            var createdProducts = new List<SimpleProduct>();

            // Validate all lines first
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    invalidLines.Add($"Line {i + 1}: Missing code or name (must be in format: Code, Name)");
                }
                else
                {
                    var code = parts[0].Trim();
                    var name = string.Join(',', parts.Skip(1)).Trim();

                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                    {
                        invalidLines.Add($"Line {i + 1}: Missing code or name");
                    }
                }
            }

            if (invalidLines.Any())
            {
                result.ErrorMessage = string.Join("; ", invalidLines);
                BulkCreateResult = result;
                return Page();
            }

            // Process valid lines
            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var code = parts[0].Trim();
                    var name = string.Join(',', parts.Skip(1)).Trim();

                    if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
                    {
                        var product = new Product
                        {
                            Id = Guid.NewGuid(),
                            ProductCode = code,
                            Name = name,
                            Type = BulkProductType,
                            Condition = string.IsNullOrWhiteSpace(BulkCondition) ? "New" : BulkCondition,
                            Group = BulkGroup,
                            WarehouseId = warehouseId,
                            Barcode = BulkAutoGenerateBarcodes ? code : string.Empty,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _productRepository.AddAsync(product);
                        createdProducts.Add(new SimpleProduct { Code = code, Name = name });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.CreatedCount = createdProducts.Count;
            result.CreatedProducts = createdProducts;
            TempData["Message"] = $"Successfully created {createdProducts.Count} products.";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        BulkCreateResult = result;
        TempData["BulkCreateResult"] = JsonSerializer.Serialize(result);
        return RedirectToPage(new { activeTab = "bulk" });
    }
}

public class UserSettingsViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
    public bool EnableEmailNotifications { get; set; }
    public int SessionTimeout { get; set; } = 30;
    public bool RequireTwoFactor { get; set; }
}

public class ProductSettingsViewModel
{
    public int ItemsPerPage { get; set; } = 5;
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
}

public class ReportSettingsViewModel
{
    public string DefaultDateRange { get; set; } = "30";
    public string DefaultReportType { get; set; } = "StockLevels";
    public bool IncludeZeroStock { get; set; }
    public bool AutoRefreshReports { get; set; }
    public string ExportFormat { get; set; } = "CSV";
    public int WarningDaysThreshold { get; set; } = 30;
    public bool EnableExpiryEmails { get; set; }
}

public class BulkCreateResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int CreatedCount { get; set; }
    public List<SimpleProduct> CreatedProducts { get; set; } = new();
}

public class SimpleProduct
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
