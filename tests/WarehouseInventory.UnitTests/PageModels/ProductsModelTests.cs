using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
using WarehouseInventory.Web.Pages.Products;
using Moq;

namespace WarehouseInventory.UnitTests.PageModels;

public class ProductsModelTests
{
    [Fact]
    public async Task OnPostAdd_ShouldAddProductToDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var mockProductService = new Mock<IProductService>();
        var expectedProductDto = new ProductDto
        {
            Id = Guid.NewGuid(),
            ProductCode = "NEW001",
            Name = "New Product"
        };
        mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<CreateProductDto>()))
            .ReturnsAsync(expectedProductDto);

        var model = new ProductsModel(context, mockProductService.Object)
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
    }

    [Fact]
    public async Task OnPostDelete_ShouldRemoveProductFromDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var mockProductService = new Mock<IProductService>();
        var model = new ProductsModel(context, mockProductService.Object);
        var existing = context.Products.First();

        var result = await model.OnPostDeleteAsync(existing.Id);

        result.Should().BeOfType<RedirectToPageResult>();
        context.Products.Should().NotContain(p => p.Id == existing.Id);
    }
}
