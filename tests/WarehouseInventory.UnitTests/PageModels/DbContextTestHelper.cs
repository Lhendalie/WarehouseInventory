using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Infrastructure.Data;

namespace WarehouseInventory.UnitTests.PageModels;

internal static class DbContextTestHelper
{
    public static async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        await DatabaseInitializer.InitializeAsync(context);
        return context;
    }
}
