using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Web.Pages.Products;

namespace WarehouseInventory.UnitTests.PageModels;

public class ProductsModelTests
{
    [Fact]
    public async Task OnPostAdd_ShouldAddProductToDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new ProductsModel(context)
        {
            NewProduct = new ProductViewModel
            {
                ProductCode = "NEW001",
                ProductName = "New Product",
                Group = "Demo",
                Barcode = "1111111111111",
                Catalogue = "CAT-NEW",
                Warehouse = "Main Warehouse",
                Type = "Tools",
                Condition = "New",
                Date = DateTime.UtcNow,
                ExpiryDate = null
            }
        };

        var result = await model.OnPostAddAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        context.Products.Should().Contain(p => p.ProductCode == "NEW001" && p.Name == "New Product");
    }

    [Fact]
    public async Task OnPostDelete_ShouldRemoveProductFromDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new ProductsModel(context);
        var existing = context.Products.First();

        var result = await model.OnPostDeleteAsync(existing.Id);

        result.Should().BeOfType<RedirectToPageResult>();
        context.Products.Should().NotContain(p => p.Id == existing.Id);
    }
}
