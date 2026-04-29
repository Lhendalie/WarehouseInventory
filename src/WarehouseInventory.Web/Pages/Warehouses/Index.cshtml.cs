using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
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
    private readonly IWarehouseService _warehouseService;

    public WarehousesModel(ApplicationDbContext context, IWarehouseService warehouseService)
    {
        _context = context;
        _warehouseService = warehouseService;
    }

    public List<WarehouseViewModel> Warehouses { get; set; } = new();

    [BindProperty]
    public WarehouseViewModel NewWarehouse { get; set; } = new();

    [BindProperty]
    public WarehouseViewModel EditWarehouse { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public bool ShowAddModal { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public bool ShowEditModal { get; set; } = false;

    public Guid? SelectedWarehouseId { get; set; }
    public WarehouseViewModel? SelectedWarehouse { get; set; }

    public async Task OnGetAsync(Guid? selectedWarehouseId = null)
    {
        await LoadWarehousesAsync(selectedWarehouseId);
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var dto = new CreateWarehouseDto
        {
            Name = NewWarehouse.Name,
            Location = NewWarehouse.Address
        };

        try
        {
            var result = await _warehouseService.CreateWarehouseAsync(dto);
            return RedirectToPage(new { selectedWarehouseId = result.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ShowAddModal = true;
            await LoadWarehousesAsync(SelectedWarehouseId);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var dto = new UpdateWarehouseDto
        {
            Id = EditWarehouse.Id,
            Name = EditWarehouse.Name,
            Location = EditWarehouse.Address,
            IsActive = true
        };

        try
        {
            await _warehouseService.UpdateWarehouseAsync(dto);
            return RedirectToPage(new { selectedWarehouseId = EditWarehouse.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ShowEditModal = true;
            await LoadWarehousesAsync(EditWarehouse.Id);
            return Page();
        }
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
