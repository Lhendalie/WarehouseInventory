using FluentAssertions;
using Moq;
using WarehouseInventory.Infrastructure.Services;
using WarehouseInventory.Web.Pages;

namespace WarehouseInventory.UnitTests.PageModels;

public class IndexModelTests
{
    [Fact]
    public async Task OnGet_ShouldLoadDashboardMetrics_FromDatabase()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var emailService = new Mock<IEmailService>();
        var model = new IndexModel(emailService.Object, context);

        await model.OnGetAsync();

        model.TotalProducts.Should().Be(context.Products.Count());
        model.TotalWarehouses.Should().Be(context.Warehouses.Count());
        model.TotalStock.Should().Be((int)Math.Round(context.ProductStocks.Sum(s => s.Quantity)));
        model.ExpiringSoon.Should().Be(context.Products.Count(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30)));
    }

    [Fact]
    public async Task OnPostSendExpiryEmail_ShouldSetNoProductsMessage_WhenNothingExpiresTomorrow()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var store = context.Products.ToList();
        foreach (var product in store)
        {
            product.ExpiryDate = null;
        }
        await context.SaveChangesAsync();

        var emailService = new Mock<IEmailService>();
        var model = new IndexModel(emailService.Object, context)
        {
            RecipientEmail = "test@example.com"
        };

        var result = await model.OnPostSendExpiryEmail();

        result.Should().NotBeNull();
        model.EmailMessage.Should().Be("No products expiring tomorrow.");
        model.EmailSent.Should().BeFalse();
        emailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
