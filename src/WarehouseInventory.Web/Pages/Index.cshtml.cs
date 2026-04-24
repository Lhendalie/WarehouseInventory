using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Infrastructure.Services;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;

    public IndexModel(IEmailService emailService, ApplicationDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    [BindProperty]
    public string RecipientEmail { get; set; } = string.Empty;

    public string EmailMessage { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalStock { get; set; }
    public int ExpiringSoon { get; set; }

    public async Task OnGetAsync()
    {
        EmailMessage = string.Empty;
        EmailSent = false;
        ErrorMessage = string.Empty;
        await LoadDashboardMetricsAsync();
    }

    public async Task<IActionResult> OnPostSendExpiryEmail()
    {
        await LoadDashboardMetricsAsync();

        if (string.IsNullOrWhiteSpace(RecipientEmail))
        {
            ErrorMessage = "Please provide a recipient email address.";
            return Page();
        }

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var expiringProducts = await _context.Products
            .Include(p => p.Warehouse)
            .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date == tomorrow)
            .ToListAsync();

        if (expiringProducts.Any())
        {
            var emailBody = "Products expiring tomorrow:\n\n";
            foreach (var product in expiringProducts)
            {
                emailBody += $"- {product.ProductCode}: {product.Name} (Type: {product.Type}, Warehouse: {product.Warehouse?.Name})\n";
            }

            try
            {
                await _emailService.SendEmailAsync(RecipientEmail, "Warehouse Inventory - Products Expiring Tomorrow", emailBody);
                EmailMessage = emailBody;
                EmailSent = true;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to send email: {ex.Message}";
                EmailMessage = emailBody;
                EmailSent = false;
            }
        }
        else
        {
            EmailMessage = "No products expiring tomorrow.";
            EmailSent = false;
            ErrorMessage = string.Empty;
        }

        return Page();
    }

    private async Task LoadDashboardMetricsAsync()
    {
        TotalProducts = await _context.Products.CountAsync();
        TotalWarehouses = await _context.Warehouses.CountAsync();
        TotalStock = (int)Math.Round(await _context.ProductStocks.SumAsync(s => (decimal?)s.Quantity) ?? 0m);
        ExpiringSoon = await _context.Products.CountAsync(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30));
    }
}
