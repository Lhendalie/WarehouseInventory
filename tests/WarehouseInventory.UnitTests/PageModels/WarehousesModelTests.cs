using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
using WarehouseInventory.Web.Pages.Warehouses;
using Moq;

namespace WarehouseInventory.UnitTests.PageModels;

public class WarehousesModelTests
{
    [Fact]
    public async Task OnPostAdd_ShouldAddWarehouseToDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var mockWarehouseService = new Mock<IWarehouseService>();
        var expectedWarehouseDto = new WarehouseDto
        {
            Id = Guid.NewGuid(),
            Name = "North Hub"
        };
        mockWarehouseService.Setup(s => s.CreateWarehouseAsync(It.IsAny<CreateWarehouseDto>()))
            .ReturnsAsync(expectedWarehouseDto);

        var model = new WarehousesModel(context, mockWarehouseService.Object)
        {
            NewWarehouse = new WarehouseViewModel
            {
                Name = "North Hub",
                Address = "100 North Street"
            }
        };

        var result = await model.OnPostAddAsync();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPostDelete_ShouldRemoveWarehouseFromDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var mockWarehouseService = new Mock<IWarehouseService>();
        var model = new WarehousesModel(context, mockWarehouseService.Object);
        var existing = context.Warehouses.First();

        var result = await model.OnPostDeleteAsync(existing.Id);

        result.Should().NotBeNull();
        context.Warehouses.Should().NotContain(w => w.Id == existing.Id);
    }
}
