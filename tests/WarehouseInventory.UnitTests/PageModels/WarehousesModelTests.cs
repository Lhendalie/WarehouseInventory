using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WarehouseInventory.Web.Pages.Warehouses;

namespace WarehouseInventory.UnitTests.PageModels;

public class WarehousesModelTests
{
    [Fact]
    public async Task OnPostAdd_ShouldAddWarehouseToDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new WarehousesModel(context)
        {
            NewWarehouse = new WarehouseViewModel
            {
                Name = "North Hub",
                Address = "100 North Street"
            }
        };

        var result = await model.OnPostAddAsync();

        result.Should().NotBeNull();
        context.Warehouses.Should().Contain(w => w.Name == "North Hub");
    }

    [Fact]
    public async Task OnPostDelete_ShouldRemoveWarehouseFromDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new WarehousesModel(context);
        var existing = context.Warehouses.First();

        var result = await model.OnPostDeleteAsync(existing.Id);

        result.Should().NotBeNull();
        context.Warehouses.Should().NotContain(w => w.Id == existing.Id);
    }
}
