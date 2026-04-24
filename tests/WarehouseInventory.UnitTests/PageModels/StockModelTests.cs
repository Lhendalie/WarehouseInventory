using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Web.Pages.Stock;

namespace WarehouseInventory.UnitTests.PageModels;

public class StockModelTests
{
    [Fact]
    public async Task OnPostStockIn_ShouldIncreaseQuantity_AndPreserveFiltersInRedirect()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new StockModel(context);
        var item = context.ProductStocks.First();
        var originalQuantity = item.Quantity;

        var result = await model.OnPostStockInAsync(item.Id, 5, "Main Warehouse", "Widget");

        context.ProductStocks.First(s => s.Id == item.Id).Quantity.Should().Be(originalQuantity + 5);
        var redirect = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirect.RouteValues.Should().ContainKey("filterWarehouse");
        redirect.RouteValues!["filterWarehouse"].Should().Be("Main Warehouse");
        redirect.RouteValues["searchTerm"].Should().Be("Widget");
    }

    [Fact]
    public async Task OnPostStockOut_ShouldDecreaseQuantity_WhenEnoughStockExists()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new StockModel(context);
        var item = context.ProductStocks.First(s => s.Quantity >= 10);
        var originalQuantity = item.Quantity;

        await model.OnPostStockOutAsync(item.Id, 3, "Main Warehouse", "Widget");

        context.ProductStocks.First(s => s.Id == item.Id).Quantity.Should().Be(originalQuantity - 3);
    }
}
