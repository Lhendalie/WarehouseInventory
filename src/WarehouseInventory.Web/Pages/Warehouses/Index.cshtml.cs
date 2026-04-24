using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.Web.Pages.Warehouses;

public class WarehouseViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class WarehousesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public WarehousesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<WarehouseViewModel> Warehouses { get; set; } = new();

    [BindProperty]
    public WarehouseViewModel NewWarehouse { get; set; } = new();

    [BindProperty]
    public WarehouseViewModel EditWarehouse { get; set; } = new();

    public Guid? SelectedWarehouseId { get; set; }
    public WarehouseViewModel? SelectedWarehouse { get; set; }

    public async Task OnGetAsync(Guid? selectedWarehouseId = null)
    {
        await LoadWarehousesAsync(selectedWarehouseId);
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadWarehousesAsync(SelectedWarehouseId);
            return Page();
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = NewWarehouse.Name,
            Location = NewWarehouse.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { selectedWarehouseId = warehouse.Id });
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadWarehousesAsync(EditWarehouse.Id);
            return Page();
        }

        var existing = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == EditWarehouse.Id);
        if (existing != null)
        {
            existing.Name = EditWarehouse.Name;
            existing.Location = EditWarehouse.Address;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = Guid.Empty;
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { selectedWarehouseId = EditWarehouse.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id);
        if (warehouse != null)
        {
            var linkedProducts = await _context.Products.Where(p => p.WarehouseId == id).ToListAsync();
            foreach (var product in linkedProducts)
            {
                product.WarehouseId = null;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = Guid.Empty;
            }

            var linkedStocks = await _context.ProductStocks.Where(s => s.WarehouseId == id).ToListAsync();
            var linkedTransactions = await _context.StockTransactions.Where(t => t.WarehouseId == id).ToListAsync();
            _context.StockTransactions.RemoveRange(linkedTransactions);
            _context.ProductStocks.RemoveRange(linkedStocks);
            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadWarehousesAsync(Guid? selectedWarehouseId)
    {
        Warehouses = await _context.Warehouses
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseViewModel
            {
                Id = w.Id,
                Name = w.Name,
                Address = w.Location ?? string.Empty
            })
            .ToListAsync();
        SelectedWarehouseId = selectedWarehouseId;
        SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == selectedWarehouseId);

        if (SelectedWarehouse != null)
        {
            EditWarehouse = new WarehouseViewModel
            {
                Id = SelectedWarehouse.Id,
                Name = SelectedWarehouse.Name,
                Address = SelectedWarehouse.Address
            };
        }
    }
}
